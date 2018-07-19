using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Models;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared;
using HeyImIn.Shared.Tests;
using HeyImIn.WebApplication.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Services
{
	public class CronSendMissedChatMessagesServiceTests : TestBase
	{
		public CronSendMissedChatMessagesServiceTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task RunAsync_GivenNoUnreadMessages_NoNotificationsSent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			DateTime sentDate = DateTime.UtcNow - TimeSpan.FromHours(1);

			// Arrange
			IEnumerable<ChatMessage> CreateMessagesFunc(User john, User richard)
			{

				yield return new ChatMessage { Author = john, Content = "Some fancy message", SentDate = sentDate };
				yield return new ChatMessage { Author = richard, Content = "A second even fancier message", SentDate = sentDate };
			}

			await CreateTestDataAsync(getContext, sentDate, sentDate, CreateMessagesFunc);

			// Act
			(CronSendMissedChatMessagesService service, _) = CreateService(getContext);
			await service.RunAsync(CancellationToken.None);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				List<EventParticipation> participations = await context.EventParticipations.ToListAsync();
				Assert.Equal(2, participations.Count);
				foreach (EventParticipation eventParticipation in participations)
				{
					Assert.Equal(sentDate, eventParticipation.LastReadMessageSentDate);
				}
			}
		}

		[Fact]
		public async Task RunAsync_GivenVeryRecentlySentUnreadMessages_NoNotificationsSent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			DateTime sentDate = DateTime.UtcNow - TimeSpan.FromSeconds(10);
			DateTime lastReadTime = DateTime.UtcNow - TimeSpan.FromSeconds(30);

			// Arrange
			IEnumerable<ChatMessage> CreateMessagesFunc(User john, User richard)
			{

				yield return new ChatMessage { Author = john, Content = "Some fancy message", SentDate = sentDate };
				yield return new ChatMessage { Author = richard, Content = "A second even fancier message", SentDate = sentDate };
			}

			await CreateTestDataAsync(getContext, lastReadTime, lastReadTime, CreateMessagesFunc);

			// Act
			(CronSendMissedChatMessagesService service, _) = CreateService(getContext);
			await service.RunAsync(CancellationToken.None);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				List<EventParticipation> participations = await context.EventParticipations.ToListAsync();
				Assert.Equal(2, participations.Count);
				foreach (EventParticipation eventParticipation in participations)
				{
					Assert.Equal(lastReadTime, eventParticipation.LastReadMessageSentDate);
				}
			}
		}

		[Fact]
		public async Task RunAsync_GivenUnreadMessages_NotificationsSent()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			DateTime latestSentDate = DateTime.UtcNow - TimeSpan.FromHours(1);

			// Arrange
			IEnumerable<ChatMessage> CreateMessagesFunc(User john, User richard)
			{
				yield return new ChatMessage { Author = john, Content = "Some early message", SentDate = latestSentDate - TimeSpan.FromSeconds(60) };
				yield return new ChatMessage { Author = john, Content = "Some late message", SentDate = latestSentDate };
				yield return new ChatMessage { Author = richard, Content = "Some average message", SentDate = latestSentDate - TimeSpan.FromSeconds(30) };
			}

			Event @event = await CreateTestDataAsync(getContext, latestSentDate - TimeSpan.FromHours(1), DateTime.MinValue, CreateMessagesFunc);

			// Act
			(CronSendMissedChatMessagesService service, Mock<AssertingNotificationService> notificationServiceMock) = CreateService(getContext);
			Expression<Func<AssertingNotificationService, Task>> mockExpression =
				n => n.NotifyUnreadChatMessagesAsync(It.Is<ChatMessagesNotificationInformation>(
					a => (a.EventId == @event.Id) && (a.Messages.Count == 3)));

			notificationServiceMock.Setup(mockExpression).CallBase();
			await service.RunAsync(CancellationToken.None);

			// Assert
			notificationServiceMock.Verify(mockExpression, Times.Exactly(2));
			using (IDatabaseContext context = getContext())
			{
				List<EventParticipation> participations = await context.EventParticipations.ToListAsync();
				Assert.Equal(2, participations.Count);
				foreach (EventParticipation eventParticipation in participations)
				{
					Assert.Equal(latestSentDate, eventParticipation.LastReadMessageSentDate);
				}
			}
		}

		private static async Task<Event> CreateTestDataAsync(GetDatabaseContext getContext, DateTime lastReadJohn, DateTime lastReadRichard, Func<User, User, IEnumerable<ChatMessage>> createMessagesFunc)
		{
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				var @event = new Event
				{
					Title = "Chatty event",
					Description = "An event with an active chat",
					MeetingPlace = "Somewhere",
					Organizer = john,
					ReminderTimeWindowInHours = 42,
					SummaryTimeWindowInHours = 24,
					EventParticipations = new List<EventParticipation>
					{
						new EventParticipation { Participant = john, LastReadMessageSentDate = lastReadJohn },
						new EventParticipation { Participant = richard, LastReadMessageSentDate = lastReadRichard }
					},
					ChatMessages = createMessagesFunc(john, richard).ToList()
				};

				context.Events.Add(@event);

				await context.SaveChangesAsync();

				return @event;
			}
		}

		private static (CronSendMissedChatMessagesService service, Mock<AssertingNotificationService> notificationServiceMock) CreateService(GetDatabaseContext getContext)
		{
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);
			var service = new CronSendMissedChatMessagesService(notificationServiceMock.Object, new HeyImInConfiguration(), getContext());
			return (service, notificationServiceMock);
		}
	}
}
