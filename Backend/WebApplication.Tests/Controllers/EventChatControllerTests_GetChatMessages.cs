using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class EventChatControllerTests
	{
		[Fact]
		public async Task GetChatMessage_GivenNotParticipatingUser_ErrorReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event privateEvent = DummyEvent(john.Entity);
				context.Events.Add(privateEvent);

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = privateEvent.Id;
			}

			// Act
			(EventChatController eventChatController, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await eventChatController.GetChatMessages(new GetChatMessagesDto { EventId = eventId });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.UserNotPartOfEvent, objectResult.Value);
		}

		[Fact]
		public async Task GetChatMessage_GivenInitialDataLoad_SubsetOfChatMessagesReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;
			ChatMessage veryOldMessage;
			ChatMessage veryRecentMessage;
			int amountOfChatMessages = _configuration.BaseAmountOfChatMessagesPerDetailPage = 5;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event dummyEvent = DummyEvent(john.Entity);
				context.Events.Add(dummyEvent);

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = dummyEvent });

				// Add 2 more chat messages then we'll request to ensure it's based on the sent date
				veryOldMessage = new ChatMessage
				{
					Event = dummyEvent,
					Author = john.Entity,
					Content = "Very old chat message",
					SentDate = DateTime.UtcNow - TimeSpan.FromDays(50)
				};
				veryRecentMessage = new ChatMessage
				{
					Event = dummyEvent,
					Author = john.Entity,
					Content = "Very recent chat message",
					SentDate = DateTime.UtcNow
				};
				context.ChatMessages.Add(veryOldMessage);
				context.ChatMessages.Add(veryRecentMessage);

				for (var i = 1; i <= amountOfChatMessages; i++)
				{
					context.ChatMessages.Add(new ChatMessage
					{
						Event = dummyEvent,
						Author = john.Entity,
						Content = "Chat message #" + i,
						SentDate = DateTime.UtcNow - TimeSpan.FromHours(i)
					});
				}

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = dummyEvent.Id;
			}

			// Act
			(EventChatController eventChatController, _) = CreateController(getContext, johnDoeId);

			IActionResult response = await eventChatController.GetChatMessages(new GetChatMessagesDto { EventId = eventId });

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventChatMessages = okObjectResult.Value as EventChatMessages;
			Assert.NotNull(eventChatMessages);
			Assert.True(eventChatMessages.PossiblyMoreMessages);
			Assert.Equal(amountOfChatMessages, eventChatMessages.Messages.Count);
			Assert.Contains(eventChatMessages.Messages, m => m.SentDate == veryRecentMessage.SentDate);
			Assert.DoesNotContain(eventChatMessages.Messages, m => m.SentDate == veryOldMessage.SentDate);
			using (IDatabaseContext context = getContext())
			{
				EventParticipation loadedParticipation = await context.EventParticipations.SingleAsync();
				Assert.Equal(veryRecentMessage.SentDate, loadedParticipation.LastReadMessageSentDate);
			}
		}


		[Fact]
		public async Task GetChatMessage_GivenConsecutiveDataLoad_AllChatMessagesReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int eventId;
			int amountOfChatMessages = _configuration.BaseAmountOfChatMessagesPerDetailPage = 5;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				Event dummyEvent = DummyEvent(john.Entity);
				context.Events.Add(dummyEvent);

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = dummyEvent });

				int addedAmount = amountOfChatMessages * 2 - 1;
				for (var i = 1; i <= addedAmount; i++)
				{
					context.ChatMessages.Add(new ChatMessage
					{
						Event = dummyEvent,
						Author = john.Entity,
						Content = "Chat message #" + i,
						SentDate = DateTime.UtcNow - TimeSpan.FromHours(i)
					});
				}

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				eventId = dummyEvent.Id;
			}

			// Act & Assert
			(EventChatController eventChatController, _) = CreateController(getContext, johnDoeId);

			IActionResult firstResponse = await eventChatController.GetChatMessages(new GetChatMessagesDto { EventId = eventId });
			Assert.IsType<OkObjectResult>(firstResponse);
			var firstOkObjectResult = (OkObjectResult)firstResponse;
			var firstEventChatMessages = firstOkObjectResult.Value as EventChatMessages;
			Assert.NotNull(firstEventChatMessages);
			Assert.True(firstEventChatMessages.PossiblyMoreMessages);
			Assert.Equal(amountOfChatMessages, firstEventChatMessages.Messages.Count);
			DateTime earliestDate = firstEventChatMessages.Messages.Min(m => m.SentDate);
			Assert.Equal(earliestDate, firstEventChatMessages.Messages.Last().SentDate);

			IActionResult secondResponse = await eventChatController.GetChatMessages(new GetChatMessagesDto { EventId = eventId, EarliestLoadedMessageSentDate = earliestDate});
			Assert.IsType<OkObjectResult>(secondResponse);
			var secondOkObjectResult = (OkObjectResult)secondResponse;
			var secondEventChatMessages = secondOkObjectResult.Value as EventChatMessages;
			Assert.NotNull(secondEventChatMessages);
			Assert.False(secondEventChatMessages.PossiblyMoreMessages);
			Assert.Equal(amountOfChatMessages - 1, secondEventChatMessages.Messages.Count);
		}
	}
}
