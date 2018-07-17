using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.Shared;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	[ApiController]
	[Route("api/EventChat")]
	public class EventChatController : ControllerBase
	{
		public EventChatController(INotificationService notificationService, HeyImInConfiguration configuration, GetDatabaseContext getDatabaseContext, ILogger<EventChatController> logger, ILoggerFactory loggerFactory)
		{
			_notificationService = notificationService;
			_baseAmountOfChatMessagesPerDetailPage = configuration.BaseAmountOfChatMessagesPerDetailPage;
			_getDatabaseContext = getDatabaseContext;
			_logger = logger;
			_auditLogger = loggerFactory.CreateAuditLogger();
		}

		/// <summary>
		///     Load a subset of <see cref="EventChatMessage" /> for a specified <see cref="Event" />.
		///     You can call this method again providing a <paramref name="earliestLoadedMessageSentDate" /> to load the next chunk of
		///     messages.
		/// </summary>
		/// <param name="eventId">The <see cref="Event.Id" /></param>
		/// <param name="earliestLoadedMessageSentDate">
		///     Null or the earliest <see cref="EventChatMessage.SentDate" /> received when previously calling
		///     this method
		/// </param>
		/// <returns>
		///     <see cref="EventChatMessages" />
		/// </returns>
		[HttpGet(nameof(GetChatMessages))]
		[ProducesResponseType(typeof(EventChatMessages), 200)]
		public async Task<IActionResult> GetChatMessages(int eventId, DateTime? earliestLoadedMessageSentDate = null)
		{
			IDatabaseContext context = _getDatabaseContext();
			int currentUserId = HttpContext.GetUserId();

			EventParticipation eventParticipation = await context.EventParticipations.FirstOrDefaultAsync(ep => (ep.EventId == eventId) && (ep.ParticipantId == currentUserId));

			if (eventParticipation == null)
			{
				return BadRequest(RequestStringMessages.UserNotPartOfEvent);
			}

			IQueryable<ChatMessage> chatMessagesQuery = context.ChatMessages.Where(c => c.EventId == eventId);

			if (earliestLoadedMessageSentDate.HasValue)
			{
				chatMessagesQuery = chatMessagesQuery.Where(c => c.SentDate < earliestLoadedMessageSentDate);
			}

			List<ChatMessage> chatMessages = await chatMessagesQuery
				.OrderByDescending(c => c.SentDate)
				.Take(_baseAmountOfChatMessagesPerDetailPage)
				.ToListAsync();

			List<EventChatMessage> eventChatMessages = chatMessages.Select(m => new EventChatMessage(m.AuthorId, m.Content, m.SentDate)).ToList();

			if (earliestLoadedMessageSentDate == null)
			{
				DateTime? lastReadMessageSentDate = chatMessages.FirstOrDefault()?.SentDate;

				if (lastReadMessageSentDate.HasValue && (eventParticipation.LastReadMessageSentDate != lastReadMessageSentDate.Value))
				{
					_logger.LogDebug("{0}(): Automatically marking loaded messages as read", nameof(GetChatMessages));
					// The user loaded the latest messages => Assume he also read them
					eventParticipation.LastReadMessageSentDate = lastReadMessageSentDate.Value;
					await context.SaveChangesAsync();
				}
			}

			return Ok(new EventChatMessages(eventChatMessages, chatMessages.Count == _baseAmountOfChatMessagesPerDetailPage));
		}

		/// <summary>
		///     Sends a <see cref="ChatMessage" /> to an <see cref="Event" />
		/// </summary>
		[HttpPost(nameof(SendChatMessage))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> SendChatMessage(SendMessageDto sendMessageDto)
		{
			IDatabaseContext context = _getDatabaseContext();
			int currentUserId = HttpContext.GetUserId();

			List<EventParticipation> eventParticipations = await context.EventParticipations.Where(e => e.EventId == sendMessageDto.EventId).ToListAsync();
			EventParticipation currentUserParticipation = eventParticipations.FirstOrDefault(e => e.ParticipantId == currentUserId);

			if (currentUserParticipation == null)
			{
				return BadRequest(RequestStringMessages.UserNotPartOfEvent);
			}

			var message = new ChatMessage
			{
				AuthorId = currentUserId,
				EventId = sendMessageDto.EventId,
				Content = sendMessageDto.Content,
				SentDate = DateTime.UtcNow
			};

			context.ChatMessages.Add(message);

			currentUserParticipation.LastReadMessageSentDate = message.SentDate;

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Sent a new chat message (containing {1} characters) to the event {2}", nameof(SendChatMessage), sendMessageDto.Content.Length, sendMessageDto.EventId);

			// TODO Push-Notifications eventParticipations.Where(e => e.ParticipantId != currentUserId).ForEach(p => SendNotification(p));

			return Ok();
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private readonly ILogger<EventChatController> _logger;
		private readonly ILogger _auditLogger;

		private readonly int _baseAmountOfChatMessagesPerDetailPage;
	}
}
