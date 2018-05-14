using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests
	{
		[Fact]
		public async Task GetDetails_GivenNonexistentEvent_NotFoundReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
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

			IActionResult response = await participateEventController.GetDetails(1);

			// Assert
			Assert.IsType<NotFoundResult>(response);
		}

		[Fact]
		public async Task GetDetails_GivenPublicEvent_DetailsReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int publicEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User richard = ContextUtilities.CreateRichardRoe();
				Event publicEvent = DummyEvent(richard);
				context.Events.Add(publicEvent);

				await context.SaveChangesAsync();

				publicEventId = publicEvent.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetDetails(publicEventId);

			// Assert
			Assert.IsType<OkObjectResult>(response);
		}

		[Fact]
		public async Task GetDetails_GivenPrivateEvent_InvitationRequiredReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int privateEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User richard = ContextUtilities.CreateRichardRoe();
				Event privateEvent = DummyEvent(richard, true);
				context.Events.Add(privateEvent);

				await context.SaveChangesAsync();

				privateEventId = privateEvent.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetDetails(privateEventId);

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.InvitationRequired, objectResult.Value);
		}

		[Fact]
		public async Task GetDetails_GivenPrivateEventAsOrganizer_DetailsReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int privateEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				Event privateEvent = DummyEvent(john.Entity, true);
				context.Events.Add(privateEvent);

				await context.SaveChangesAsync();

				privateEventId = privateEvent.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetDetails(privateEventId);

			// Assert
			Assert.IsType<OkObjectResult>(response);
		}



		[Fact]
		public async Task GetDetails_GivenPrivateEventAsParticipator_DetailsReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			int privateEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User richard = ContextUtilities.CreateRichardRoe();
				Event privateEvent = DummyEvent(richard, true);
				context.Events.Add(privateEvent);
				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = privateEvent });

				await context.SaveChangesAsync();

				privateEventId = privateEvent.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetDetails(privateEventId);

			// Assert
			Assert.IsType<OkObjectResult>(response);
		}
	}
}
