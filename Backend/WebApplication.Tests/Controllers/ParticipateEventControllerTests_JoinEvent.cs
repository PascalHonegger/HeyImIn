using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests
	{
		[Fact]
		public async Task JoinEvent_GivenNonParticipatingUser_UserCanJoin()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event @event = DummyEvent(ContextUtilities.CreateRichardRoe());
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await participateEventController.JoinEvent(new JoinEventDto
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
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event @event = DummyEvent(ContextUtilities.CreateRichardRoe());
				context.Events.Add(@event);

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = @event });

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await participateEventController.JoinEvent(new JoinEventDto
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
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event privateEvent = DummyEvent(ContextUtilities.CreateRichardRoe(), true);
				context.Events.Add(privateEvent);

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = privateEvent.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await participateEventController.JoinEvent(new JoinEventDto
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
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int organizerId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				Event @event = DummyEvent(ContextUtilities.CreateJohnDoe(), true);
				context.Events.Add(@event);

				await context.SaveChangesAsync();

				organizerId = @event.OrganizerId;
				eventId = @event.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, organizerId);

			IActionResult response = await participateEventController.JoinEvent(new JoinEventDto
			{
				EventId = eventId
			});

			// Assert
			Assert.IsType<OkResult>(response);
		}
	}
}
