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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	[ApiController]
	[ApiVersion(ApiVersions.Version2_0)]
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
		///     You can call this method again providing a <see cref="GetChatMessagesDto.EarliestLoadedMessageSentDate" /> to load the next chunk of
		///     messages.
		/// </summary>
		/// <param name="getChatMessagesDto"><see cref="GetChatMessagesDto"/></param>
		/// <returns>
		///     <see cref="EventChatMessages" />
		/// </returns>
		[HttpPost(nameof(GetChatMessages))]
		[ProducesResponseType(typeof(EventChatMessages), 200)]
		public async Task<IActionResult> GetChatMessages(GetChatMessagesDto getChatMessagesDto)
		{
			IDatabaseContext context = _getDatabaseContext();
			int currentUserId = HttpContext.GetUserId();

			EventParticipation eventParticipation = await context.EventParticipations.FirstOrDefaultAsync(ep => (ep.EventId == getChatMessagesDto.EventId) && (ep.ParticipantId == currentUserId));

			if (eventParticipation == null)
			{
				return BadRequest(RequestStringMessages.UserNotPartOfEvent);
			}

			IQueryable<ChatMessage> chatMessagesQuery = context.ChatMessages
				.Where(c => c.EventId == getChatMessagesDto.EventId);

			bool providedAlreadyLoadedMessageSentDate = getChatMessagesDto.EarliestLoadedMessageSentDate.HasValue;
			if (providedAlreadyLoadedMessageSentDate)
			{
				DateTime dateValue = getChatMessagesDto.EarliestLoadedMessageSentDate.Value;

				chatMessagesQuery = chatMessagesQuery.Where(c => c.SentDate < dateValue);
			}

			List<EventChatMessage> eventChatMessages = await chatMessagesQuery
				.OrderByDescending(c => c.SentDate)
				.Take(_baseAmountOfChatMessagesPerDetailPage)
				.Select(m => new EventChatMessage(m.Id, m.AuthorId, m.Content, m.SentDate))
				.ToListAsync();

			if (!providedAlreadyLoadedMessageSentDate)
			{
				DateTime? lastReadMessageSentDate = eventChatMessages.FirstOrDefault()?.SentDate;

				if (lastReadMessageSentDate.HasValue && (eventParticipation.LastReadMessageSentDate != lastReadMessageSentDate.Value))
				{
					_logger.LogDebug("{0}(): Automatically marking loaded messages as read", nameof(GetChatMessages));
					// The user loaded the latest messages => Assume he also read them
					eventParticipation.LastReadMessageSentDate = lastReadMessageSentDate.Value;
					await context.SaveChangesAsync();
				}
			}

			return Ok(new EventChatMessages(eventChatMessages, eventChatMessages.Count == _baseAmountOfChatMessagesPerDetailPage));
		}

		/// <summary>
		///     Sends a <see cref="ChatMessage" /> to an <see cref="Event" />
		/// </summary>
		/// <returns>The newly added <see cref="EventChatMessage"/></returns>
		[HttpPost(nameof(SendChatMessage))]
		[ProducesResponseType(typeof(EventChatMessage), 200)]
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

			return Ok(new EventChatMessage(message.Id, currentUserId, message.Content, message.SentDate));
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private readonly ILogger<EventChatController> _logger;
		private readonly ILogger _auditLogger;

		private readonly int _baseAmountOfChatMessagesPerDetailPage;
	}
}
