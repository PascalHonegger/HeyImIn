using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests
	{
		[Fact]
		public async Task ConfigureNotifications_GivenParticipatingEvent_NotificationsConfigured()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int eventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User organizer = ContextUtilities.CreateRichardRoe();
				Event @event = DummyEvent(organizer);
				context.Events.Add(@event);
				context.EventParticipations.Add(new EventParticipation { Event = @event, Participant = john.Entity, SendLastMinuteChangesEmail = false, SendReminderEmail = false, SendSummaryEmail = false });

				await context.SaveChangesAsync();

				eventId = @event.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.ConfigureNotifications(new NotificationConfigurationDto { EventId = eventId, SendReminderEmail = true, SendSummaryEmail = true, SendLastMinuteChangesEmail = true });

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				EventParticipation eventParticipation = context.EventParticipations.Single();
				Assert.True(eventParticipation.SendSummaryEmail);
				Assert.True(eventParticipation.SendLastMinuteChangesEmail);
				Assert.True(eventParticipation.SendReminderEmail);
			}
		}

		[Fact]
		public async Task ConfigureNotifications_GivenEventNotParticipating_ErrorReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int eventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User organizer = ContextUtilities.CreateRichardRoe();
				Event @event = DummyEvent(organizer);
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				eventId = @event.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.ConfigureNotifications(new NotificationConfigurationDto { EventId = eventId, SendReminderEmail = true, SendSummaryEmail = true, SendLastMinuteChangesEmail = true });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.UserNotPartOfEvent, objectResult.Value);
		}

		[Fact]
		public async Task ConfigureNotifications_GivenNonexistentEvent_ErrorReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				await context.SaveChangesAsync();

				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.ConfigureNotifications(new NotificationConfigurationDto { EventId = 42, SendReminderEmail = true, SendSummaryEmail = true, SendLastMinuteChangesEmail = true });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.UserNotPartOfEvent, objectResult.Value);
		}
	}
}
