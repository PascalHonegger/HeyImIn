using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.MailNotifier;
using HeyImIn.Shared;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Services.Impl
{
	/// <summary>
	///     Sends notifications for missed chat messages.
	///     Chat messages are sent in bulk to prevent spam.
	/// </summary>
	public class CronSendMissedChatMessagesService : ICronService
	{
		public CronSendMissedChatMessagesService(INotificationService notificationService, HeyImInConfiguration configuration, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_minimumChatMessageNotificationTimeSpan = configuration.TimeSpans.MinimumChatMessageNotificationTimeSpan;
			_getDatabaseContext = getDatabaseContext;
		}

		public async Task RunAsync(CancellationToken token)
		{
			IDatabaseContext context = _getDatabaseContext();

			DateTime maxAge = DateTime.UtcNow - _minimumChatMessageNotificationTimeSpan;

			var chatMessagesToNotifyAbout = await context.EventParticipations
				.Select(ep => new
				{
					participant = ep.Participant,
					eventId = ep.EventId,
					eventTitle = ep.Event.Title,
					messages = ep.Event.ChatMessages
						.AsQueryable()
						.Where(c => c.SentDate < maxAge)
						.Where(c => c.SentDate > ep.LastReadMessageSentDate)
						.OrderByDescending(c => c.SentDate)
				})
				.Where(x => x.messages.AsQueryable().Any())
				.ToListAsync(token);

			/*
			await context.ChatMessages
				.OrderByDescending(c => c.SentDate)
				.SkipWhile(c => c.Id != ep.LastReadChatMessageId)
				.ToListAsync();

			var appointmentsWithPossibleReminders = await context.ChatMessages
				.Select(c => c.EventParticipations.Except(c.Event.EventParticipations))
				.Where(a => a.StartTime.Add(_minimumChatMessageNotificationTimeSpan) <= DateTime.UtcNow)
				.ToListAsync(token);
			*/

			foreach (var chatMessage in chatMessagesToNotifyAbout)
			{
				// TODO
			}

			// Save sent chat messages
			await context.SaveChangesAsync(token);
		}

		public string DescriptiveName { get; } = "SendMissedChatMessagesCron";

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;
		private readonly TimeSpan _minimumChatMessageNotificationTimeSpan;
	}
}
