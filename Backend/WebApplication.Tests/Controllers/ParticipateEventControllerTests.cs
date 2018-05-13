using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public class ParticipateEventControllerTests : ControllerTestBase
	{
		public ParticipateEventControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		private (ParticipateEventController participateEventController, Mock<INotificationService> notificationMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var notificationServiceMock = new Mock<INotificationService>(MockBehavior.Strict);

			var controller = new ParticipateEventController(notificationServiceMock.Object, getContext, DummyLogger<ParticipateEventController>(), DummyLoggerFactory())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, notificationServiceMock);
		}

		#region JoinEvent

		[Fact]
		public async Task JoinEvent_GivenNonParticipatingUser_UserCanJoin()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.JohnDoe);

				Event @event = DummyEvent(ContextUtilities.RichardRoe);
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController authenticationService, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await authenticationService.JoinEvent(new JoinEventDto
			{
				EventId = eventId
			});

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Event loadedEvent = await context.Events.Include(e => e.EventParticipations).FirstOrDefaultAsync(e => e.Id == eventId);
				Assert.NotNull(loadedEvent);
				Assert.Contains(johnDoeId, loadedEvent.EventParticipations.Select(e => e.ParticipantId));
			}
		}

		[Fact]
		public async Task JoinEvent_GivenAlreadyParticipatingUser_UserCanNotJoin()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.JohnDoe);

				Event @event = DummyEvent(ContextUtilities.RichardRoe);
				context.Events.Add(@event);

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = @event });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController authenticationService, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await authenticationService.JoinEvent(new JoinEventDto
			{
				EventId = eventId
			});

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.UserAlreadyPartOfEvent, objectResult.Value);
		}

		[Fact]
		public async Task JoinEvent_GivenPrivateEvent_UserCanNotJoin()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.JohnDoe);

				Event @event = DummyEvent(ContextUtilities.RichardRoe, true);
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController authenticationService, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await authenticationService.JoinEvent(new JoinEventDto
			{
				EventId = eventId
			});

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.InvitationRequired, objectResult.Value);
		}

		[Fact]
		public async Task JoinEvent_GivenPrivateEvent_OrganizerCanJoin()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int organizerId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				Event @event = DummyEvent(ContextUtilities.JohnDoe, true);
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				organizerId = @event.OrganizerId;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController authenticationService, _) = CreateController(getContext, organizerId);

			IActionResult response = await authenticationService.JoinEvent(new JoinEventDto
			{
				EventId = eventId
			});

			// Assert
			Assert.IsType<OkResult>(response);
		}

		#endregion
	}
}
