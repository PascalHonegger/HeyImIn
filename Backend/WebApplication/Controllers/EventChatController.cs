using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
		///     
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="lastLoadedMessageId"></param>
		/// <returns></returns>
		[HttpGet(nameof(GetChatMessages))]
		[ProducesResponseType(typeof(EventChatMessages), 200)]
		public async Task<IActionResult> GetChatMessages(int eventId, int? lastLoadedMessageId = null)
		{
			IDatabaseContext context = _getDatabaseContext();
			int currentUserId = HttpContext.GetUserId();

			EventParticipation eventParticipation = await context.EventParticipations.FirstOrDefaultAsync(ep => (ep.EventId == eventId) && (ep.ParticipantId == currentUserId));

			if (eventParticipation == null)
			{
				return BadRequest(RequestStringMessages.UserNotPartOfEvent);
			}

			List<ChatMessage> chatMessages;
			if (lastLoadedMessageId.HasValue)
			{
				chatMessages = await context.ChatMessages
					.Where(c => c.EventId == eventId)
					.OrderByDescending(c => c.SentDate)
					.SkipWhile(c => c.Id != lastLoadedMessageId.Value)
					.Take(_baseAmountOfChatMessagesPerDetailPage)
					.ToListAsync();
			}
			else
			{
				chatMessages = await context.ChatMessages
					.Where(c => c.EventId == eventId)
					.OrderByDescending(c => c.SentDate)
					.Take(_baseAmountOfChatMessagesPerDetailPage)
					.ToListAsync();
			}

			List<EventChatMessage> eventChatMessages = chatMessages.Select(m => new EventChatMessage(m.AuthorId, m.Content, m.SentDate)).ToList();

			if (lastLoadedMessageId == null)
			{
				int? lastReadMessageId = chatMessages.FirstOrDefault()?.Id;
				// The user loaded the latest messages => Assume he also read them 
				eventParticipation.LastReadMessageId = lastReadMessageId;
				await context.SaveChangesAsync();
			}

			int? newLastLoadedMessageId = chatMessages.LastOrDefault()?.Id;

			return Ok(new EventChatMessages(eventChatMessages, chatMessages.Count == _baseAmountOfChatMessagesPerDetailPage, newLastLoadedMessageId));
		}

		/// <summary>
		///     Configures the enabled notifications of the current user for the specified event
		/// </summary>
		[HttpPost(nameof(SendChatMessage))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> SendChatMessage(SendMessageDto sendMessageDto)
		{
			IDatabaseContext context = _getDatabaseContext();
			int currentUserId = HttpContext.GetUserId();

			List<EventParticipation> eventParticipations = await context.EventParticipations.Where(e => e.EventId == sendMessageDto.EventId).ToListAsync();

			if (eventParticipations.All(e => e.ParticipantId != currentUserId))
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
