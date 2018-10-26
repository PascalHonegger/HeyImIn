using System;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Models;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class OrganizeEventControllerTests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task DeleteEvent_GivenEventUserOrganizes_EventDeleted(bool isPrivate)
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				Event @event = DummyEvent(john, isPrivate);

				context.Events.Add(@event);

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				eventId = @event.Id;
			}

			// Act
			(OrganizeEventController controller, Mock<AssertingNotificationService> notificationMock) = CreateController(getContext, johnDoeId);

			notificationMock.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId))).CallBase();

			IActionResult response = await controller.DeleteEvent(eventId);

			// Assert
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId)), Times.Once);

			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Events);
			}
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task DeleteEvent_GivenEventUserOnlyParticipates_EventNotDeleted(bool isPrivate)
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				Event @event = DummyEvent(richard, isPrivate);

				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = john });
				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = richard });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				eventId = @event.Id;
			}

			// Act
			(OrganizeEventController controller, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await controller.DeleteEvent(eventId);

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.OrganizerRequired, objectResult.Value);
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Events);
			}
		}


		[Fact]
		public async Task DeleteEvent_GivenNonExistingEvent_ErrorReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				Event @event = DummyEvent(richard, true);

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				eventId = @event.Id;
			}

			// Act
			(OrganizeEventController controller, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await controller.DeleteEvent(eventId);

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.EventNotFound, objectResult.Value);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task DeleteEvent_GivenEventWithRelations_EventDeleted(bool isPrivate)
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				Event @event = DummyEvent(john, isPrivate);

				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = john });
				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = richard });

				context.EventInvitations.Add(new EventInvitation { Event = @event, Requested = DateTime.UtcNow });

				context.ChatMessages.Add(new ChatMessage { Event = @event, Author = john, Content = "Hello World.", SentDate = DateTime.UtcNow });

				var appointment = new Appointment { Event = @event, StartTime = DateTime.UtcNow };

				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = john});
				context.AppointmentParticipations.Add(new AppointmentParticipation { Appointment = appointment, Participant = richard });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				eventId = @event.Id;
			}

			// Act
			(OrganizeEventController controller, Mock<AssertingNotificationService> notificationMock) = CreateController(getContext, johnDoeId);

			notificationMock.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId))).CallBase();

			IActionResult response = await controller.DeleteEvent(eventId);

			// Assert
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == eventId)), Times.Once);

			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Events);
				Assert.Empty(context.EventParticipations);
				Assert.Empty(context.EventInvitations);
				Assert.Empty(context.ChatMessages);
				Assert.Empty(context.Appointments);
				Assert.Empty(context.AppointmentParticipations);
			}
		}
	}
}
