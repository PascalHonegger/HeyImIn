using System;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Models;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests
	{
		[Fact]
		public async Task DeleteAccount_GivenSimpleUser_UserDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnDoeId);
			IActionResult response = await controller.DeleteAccount();

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
			}
		}

		[Fact]
		public async Task DeleteAccount_GivenUserWithSimpleRelations_UserAndRelationsDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				context.PasswordResets.Add(new PasswordReset { Requested = DateTime.UtcNow, User = john });
				context.Sessions.Add(new Session { Created = DateTime.UtcNow, User = john });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnDoeId);
			IActionResult response = await controller.DeleteAccount();

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
				Assert.Empty(context.Sessions);
				Assert.Empty(context.PasswordResets);
			}
		}

		[Fact]
		public async Task DeleteAccount_GivenUserWithEvents_UserAndEventsDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId1;
			int eventId2;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				Event event1 = DummyEvent(john);
				Event event2 = DummyEvent(john, true);
				context.EventParticipations.Add(new EventParticipation { Event = event1, Participant = john });
				context.EventParticipations.Add(new EventParticipation { Event = event2, Participant = john });

				context.EventInvitations.Add(new EventInvitation { Event = event1, Requested = DateTime.UtcNow });
				context.EventInvitations.Add(new EventInvitation { Event = event2, Requested = DateTime.UtcNow });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				eventId1 = event1.Id;
				eventId2 = event2.Id;
			}

			// Act
			(UserController controller, _, _, Mock<AssertingNotificationService> notificationMock) = CreateController(getContext, johnDoeId);

			notificationMock.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId1))).CallBase();
			notificationMock.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId2))).CallBase();

			IActionResult response = await controller.DeleteAccount();

			// Assert
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId1)), Times.Once);
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId2)), Times.Once);

			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
				Assert.Empty(context.Events);
				Assert.Empty(context.EventParticipations);
			}
		}

		[Fact]
		public async Task DeleteAccount_GivenEventOrganizedByOtherUser_PossibleLastMinuteChangesSent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int acceptedAppointmentId;
			int declinedAppointmentId;
			int noAnswerAppointmentId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				Event @event = DummyEvent(ContextUtilities.CreateRichardRoe());
				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = john });
				var acceptedAppointment = new Appointment { Event = @event, StartTime = DateTime.UtcNow };
				var declinedAppointment = new Appointment { Event = @event, StartTime = DateTime.UtcNow };
				var noAnswerAppointment = new Appointment { Event = @event, StartTime = DateTime.UtcNow };
				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = acceptedAppointment, Participant = john, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Accepted });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = declinedAppointment, Participant = john, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Declined });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = noAnswerAppointment, Participant = john, AppointmentParticipationAnswer = null });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				acceptedAppointmentId = acceptedAppointment.Id;
				declinedAppointmentId = declinedAppointment.Id;
				noAnswerAppointmentId = noAnswerAppointment.Id;
			}

			// Act
			(UserController controller, _, _, Mock<AssertingNotificationService> notificationMock) = CreateController(getContext, johnDoeId);

			notificationMock.Setup(n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(e => e.Id == acceptedAppointmentId))).CallBase();

			IActionResult response = await controller.DeleteAccount();

			// Assert
			notificationMock.Verify(n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(e => e.Id == acceptedAppointmentId)), Times.Once);
			notificationMock.Verify(n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(e => e.Id == declinedAppointmentId)), Times.Never);
			notificationMock.Verify(n => n.SendLastMinuteChangeIfRequiredAsync(It.Is<Appointment>(e => e.Id == noAnswerAppointmentId)), Times.Never);

			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Users);
				Assert.Empty(context.EventParticipations);
				Assert.Empty(context.AppointmentParticipations);
			}
		}
	}
}
