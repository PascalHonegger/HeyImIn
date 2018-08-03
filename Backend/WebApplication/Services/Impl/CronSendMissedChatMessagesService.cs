using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
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
		public CronSendMissedChatMessagesService(INotificationService notificationService, HeyImInConfiguration configuration, IDatabaseContext context)
		{
			_notificationService = notificationService;
			_minimumChatMessageNotificationTimeSpan = configuration.TimeSpans.MinimumChatMessageNotificationTimeSpan;
			_context = context;
		}

		public async Task RunAsync(CancellationToken token)
		{
			DateTime maxAge = DateTime.UtcNow - _minimumChatMessageNotificationTimeSpan;

			var chatMessagesToNotifyAbout = await _context.EventParticipations
				.Select(ep => new
				{
					participation = ep,
					eventId = ep.EventId,
					eventTitle = ep.Event.Title,
					messages = ep.Event.ChatMessages
						.Where(c => c.SentDate < maxAge)
						.Where(c => c.SentDate > ep.LastReadMessageSentDate)
						.OrderByDescending(c => c.SentDate)
						.Select(c => new ChatMessageNotificationInformation(c.AuthorId, c.SentDate, c.Content))
				})
				.Where(x => x.messages.Any())
				.ToListAsync(token);

			if (chatMessagesToNotifyAbout.Count == 0)
			{
				return;
			}

			List<int> allAuthorIds = chatMessagesToNotifyAbout
				.SelectMany(c => c.messages.Select(m => m.AuthorId))
				.Concat(chatMessagesToNotifyAbout.Select(c => c.participation.ParticipantId))
				.Distinct()
				.ToList();

			List<(int id, string fullName, string email)> relevantUsers = await _context.Users.Where(u => allAuthorIds.Contains(u.Id)).Select(u => ValueTuple.Create(u.Id, u.FullName, u.Email)).ToListAsync(token);

			foreach (var chatMessage in chatMessagesToNotifyAbout)
			{
				List<ChatMessageNotificationInformation> messages = chatMessage.messages.ToList();
				await _notificationService.NotifyUnreadChatMessagesAsync(new ChatMessagesNotificationInformation(chatMessage.eventId, chatMessage.eventTitle, chatMessage.participation.ParticipantId, messages, relevantUsers));

				chatMessage.participation.LastReadMessageSentDate = messages[0].SentDate;
			}

			// Save sent chat messages
			await _context.SaveChangesAsync(token);
		}

		public string DescriptiveName { get; } = "SendMissedChatMessagesCron";

		private readonly INotificationService _notificationService;
		private readonly IDatabaseContext _context;
		private readonly TimeSpan _minimumChatMessageNotificationTimeSpan;
	}
}
