using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests
	{
		[Fact]
		public async Task RemoveFromEvent_GivenDeclinedUser_NoUpdateSent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event @event = DummyEvent(ContextUtilities.CreateRichardRoe());

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = @event });

				var appointment = new Appointment
				{
					Event = @event,
					StartTime = DateTime.UtcNow + TimeSpan.FromDays(1)
				};

				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = john.Entity, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Declined });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = john.Entity, AppointmentParticipationAnswer = null });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController participateEventController, Mock<AssertingNotificationService> _) = CreateController(getContext, johnDoeId);

			IActionResult response = await participateEventController.RemoveFromEvent(new RemoveFromEventDto
			{
				EventId = eventId,
				UserId = johnDoeId
			});

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Events);
				Assert.Empty(context.EventParticipations);
				Assert.Single(context.Appointments);
				Assert.Empty(context.AppointmentParticipations);
			}
		}

		[Fact]
		public async Task RemoveFromEvent_GivenParticipatingUser_UserLeavesEventAndAppointments()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;
			int appointmentId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event @event = DummyEvent(ContextUtilities.CreateRichardRoe());

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = @event });

				var appointment = new Appointment
				{
					Event = @event,
					StartTime = DateTime.UtcNow + TimeSpan.FromDays(1)
				};

				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = john.Entity, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Accepted });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
				appointmentId = appointment.Id;
			}

			// Act
			(ParticipateEventController participateEventController, Mock<AssertingNotificationService> notificationServiceMock) = CreateController(getContext, johnDoeId);
			Expression<Func<AssertingNotificationService, Task>> sendLastMinuteChangeExpression = n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(a => a.Id == appointmentId));
			notificationServiceMock.Setup(sendLastMinuteChangeExpression).CallBase();

			IActionResult response = await participateEventController.RemoveFromEvent(new RemoveFromEventDto
			{
				EventId = eventId,
				UserId = johnDoeId
			});

			// Assert
			Assert.IsType<OkResult>(response);
			notificationServiceMock.Verify(sendLastMinuteChangeExpression, Times.Once);
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Events);
				Assert.Empty(context.EventParticipations);
				Assert.Single(context.Appointments);
				Assert.Empty(context.AppointmentParticipations);
			}
		}

		[Fact]
		public async Task RemoveFromEvent_GivenOrganizerUser_CanRemoveOthersFromEvent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int richardId;
			int eventId;
			int appointmentId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				EntityEntry<User> richard = context.Users.Add(ContextUtilities.CreateRichardRoe());

				Event @event = DummyEvent(john.Entity);

				context.EventParticipations.Add(new EventParticipation { Participant = richard.Entity, Event = @event });

				var appointment = new Appointment
				{
					Event = @event,
					StartTime = DateTime.UtcNow + TimeSpan.FromDays(1)
				};

				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = richard.Entity, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Accepted });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				richardId = richard.Entity.Id;
				eventId = @event.Id;
				appointmentId = appointment.Id;
			}

			// Act
			(ParticipateEventController participateEventController, Mock<AssertingNotificationService> notificationServiceMock) = CreateController(getContext, johnDoeId);

			Expression<Func<AssertingNotificationService, Task>> sendLastMinuteChangeExpression = n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(a => a.Id == appointmentId));
			Expression<Func<AssertingNotificationService, Task>> notifyOrganizerUpdatedUserExpression = n => n.NotifyOrganizerUpdatedUserInfoAsync(It.Is<Event>(e => e.Id == eventId), It.Is<User>(u => u.Id == richardId), It.IsAny<string>());

			notificationServiceMock.Setup(sendLastMinuteChangeExpression).CallBase();
			notificationServiceMock.Setup(notifyOrganizerUpdatedUserExpression).CallBase();

			IActionResult response = await participateEventController.RemoveFromEvent(new RemoveFromEventDto
			{
				EventId = eventId,
				UserId = richardId
			});

			// Assert
			Assert.IsType<OkResult>(response);
			notificationServiceMock.Verify(sendLastMinuteChangeExpression, Times.Once);
			notificationServiceMock.Verify(notifyOrganizerUpdatedUserExpression, Times.Once);
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Events);
				Assert.Empty(context.EventParticipations);
				Assert.Single(context.Appointments);
				Assert.Empty(context.AppointmentParticipations);
			}
		}

		[Fact]
		public async Task RemoveFromEvent_GivenParticipatingUser_CanNotRemoveOthersFromEvent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int richardId;
			int eventId;
			int appointmentId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				EntityEntry<User> richard = context.Users.Add(ContextUtilities.CreateRichardRoe());

				Event @event = DummyEvent(richard.Entity);

				context.EventParticipations.Add(new EventParticipation { Participant = richard.Entity, Event = @event });

				var appointment = new Appointment
				{
					Event = @event,
					StartTime = DateTime.UtcNow + TimeSpan.FromDays(1)
				};

				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = richard.Entity });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				richardId = richard.Entity.Id;
				eventId = @event.Id;
				appointmentId = appointment.Id;
			}

			// Act
			(ParticipateEventController participateEventController, Mock<AssertingNotificationService> notificationServiceMock) = CreateController(getContext, johnDoeId);

			Expression<Func<AssertingNotificationService, Task>> sendLastMinuteChangeExpression = n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(a => a.Id == appointmentId));
			Expression<Func<AssertingNotificationService, Task>> notifyOrganizerUpdatedUserExpression = n => n.NotifyOrganizerUpdatedUserInfoAsync(It.Is<Event>(e => e.Id == eventId), It.Is<User>(u => u.Id == richardId), It.IsAny<string>());

			notificationServiceMock.Setup(sendLastMinuteChangeExpression).CallBase();
			notificationServiceMock.Setup(notifyOrganizerUpdatedUserExpression).CallBase();

			IActionResult response = await participateEventController.RemoveFromEvent(new RemoveFromEventDto
			{
				EventId = eventId,
				UserId = richardId
			});

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.OrganizorRequired, objectResult.Value);

			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Events);
				Assert.Single(context.EventParticipations);
				Assert.Single(context.Appointments);
				Assert.Single(context.AppointmentParticipations);
			}
		}
	}
}
