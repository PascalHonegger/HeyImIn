using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.Shared;
using HeyImIn.WebApplication.FrontendModels;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.WebApiComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	[ApiController]
	[Route("api/OrganizeEvent")]
	public class OrganizeEventController : ControllerBase
	{
		public OrganizeEventController(INotificationService notificationService, IDeleteService deleteService, GetDatabaseContext getDatabaseContext, ILogger<OrganizeEventController> logger, ILoggerFactory loggerFactory)
		{
			_notificationService = notificationService;
			_deleteService = deleteService;
			_getDatabaseContext = getDatabaseContext;
			_logger = logger;
			_auditLogger = loggerFactory.CreateAuditLogger();
		}

		/// <summary>
		///     Deletes an event and all underlying appointments
		///     Informs the users about the deletion
		/// </summary>
		/// <param name="eventId">
		///     <see cref="Event.Id" />
		/// </param>
		[HttpDelete(nameof(DeleteEvent))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> DeleteEvent(int eventId)
		{
			IDatabaseContext context = _getDatabaseContext();
			Event @event = await context.Events
				.Include(e => e.Organizer)
				.Include(e => e.EventInvitations)
				.Include(e => e.EventParticipations)
				.Include(e => e.Appointments)
					.ThenInclude(a => a.AppointmentParticipations)
				.FirstOrDefaultAsync(e => e.Id == eventId);

			if (@event == null)
			{
				return BadRequest(RequestStringMessages.EventNotFound);
			}

			User currentUser = await HttpContext.GetCurrentUserAsync(context);

			if (@event.Organizer != currentUser)
			{
				_logger.LogInformation("{0}(): Tried to delete event {1}, which he's not organizing", nameof(DeleteEvent), @event.Id);

				return BadRequest(RequestStringMessages.OrganizerRequired);
			}

			EventNotificationInformation notificationInformation = _deleteService.DeleteEventLocally(context, @event);

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Deleted event {1}", nameof(DeleteEvent), @event.Id);

			await _notificationService.NotifyEventDeletedAsync(notificationInformation);

			return Ok();
		}

		/// <summary>
		///     Updates general information about an event
		///     Informs the users about the change
		/// </summary>
		[HttpPost(nameof(UpdateEventInfo))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> UpdateEventInfo(UpdatedEventInfoDto updatedEventInfoDto)
		{
			IDatabaseContext context = _getDatabaseContext();
			Event @event = await context.Events
				.Include(e => e.Organizer)
				.Include(e => e.EventParticipations)
				.FirstOrDefaultAsync(e => e.Id == updatedEventInfoDto.EventId);

			if (@event == null)
			{
				return BadRequest(RequestStringMessages.EventNotFound);
			}

			User currentUser = await HttpContext.GetCurrentUserAsync(context);

			if (@event.Organizer != currentUser)
			{
				_logger.LogInformation("{0}(): Tried to update event {1}, which he's not organizing", nameof(UpdateEventInfo), @event.Id);

				return BadRequest(RequestStringMessages.OrganizerRequired);
			}

			@event.Title = updatedEventInfoDto.Title;
			@event.MeetingPlace = updatedEventInfoDto.MeetingPlace;
			@event.Description = updatedEventInfoDto.Description;
			@event.IsPrivate = updatedEventInfoDto.IsPrivate;
			@event.ReminderTimeWindowInHours = updatedEventInfoDto.ReminderTimeWindowInHours;
			@event.SummaryTimeWindowInHours = updatedEventInfoDto.SummaryTimeWindowInHours;

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Updated event {1}", nameof(UpdateEventInfo), @event.Id);

			await _notificationService.NotifyEventUpdatedAsync(@event);

			return Ok();
		}

		/// <summary>
		///     Creates a new event
		/// </summary>
		/// <returns>
		///     <see cref="Event.Id" />
		/// </returns>
		[HttpPost(nameof(CreateEvent))]
		[ProducesResponseType(typeof(int), 200)]
		public async Task<IActionResult> CreateEvent(GeneralEventInformation generalEventInformation)
		{
			IDatabaseContext context = _getDatabaseContext();
			var newEvent = new Event
			{
				Title = generalEventInformation.Title,
				Description = generalEventInformation.Description,
				MeetingPlace = generalEventInformation.MeetingPlace,
				IsPrivate = generalEventInformation.IsPrivate,
				ReminderTimeWindowInHours = generalEventInformation.ReminderTimeWindowInHours,
				SummaryTimeWindowInHours = generalEventInformation.SummaryTimeWindowInHours,
				OrganizerId = HttpContext.GetUserId()
			};


			context.Events.Add(newEvent);

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Created event {1} ({2})", nameof(CreateEvent), newEvent.Id, newEvent.Title);

			return Ok(newEvent.Id);
		}

		/// <summary>
		///     Loads the details about an event which is required to edit it
		/// </summary>
		/// <param name="eventId">
		///     <see cref="Event.Id" />
		/// </param>
		/// <returns>
		///     <see cref="EditEventDetails" />
		/// </returns>
		[HttpGet(nameof(GetEditDetails))]
		[ProducesResponseType(typeof(EditEventDetails), 200)]
		public async Task<IActionResult> GetEditDetails(int eventId)
		{
			IDatabaseContext context = _getDatabaseContext();
			context.WithTrackingBehavior(QueryTrackingBehavior.NoTracking);

			int currentUserId = HttpContext.GetUserId();

			EditEventDetails @event = await context.Events
				.Where(e => (e.Id == eventId) && (e.OrganizerId == currentUserId))
				.Select(e => new EditEventDetails(
					new GeneralEventInformation(e.Title, e.MeetingPlace, e.Description, e.IsPrivate, e.ReminderTimeWindowInHours, e.SummaryTimeWindowInHours),
					e.Appointments
						.Where(a => a.StartTime >= DateTime.UtcNow)
						.OrderBy(a => a.StartTime)
						.Select(a => new AppointmentDetails(
							new AppointmentInformation(a.Id, a.StartTime),
							a.AppointmentParticipations
								.Select(ap => new AppointmentParticipationInformation(ap.ParticipantId, ap.AppointmentParticipationAnswer))
								.ToList()))
						.ToList(),
					e.EventParticipations
						.Select(ep => new UserInformation(ep.ParticipantId, ep.Participant.FullName, ep.Participant.Email))
						.ToList()))
				.SingleOrDefaultAsync();

			if (@event == null)
			{
				return NotFound();
			}

			return Ok(@event);
		}

		private readonly INotificationService _notificationService;
		private readonly IDeleteService _deleteService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private readonly ILogger<OrganizeEventController> _logger;
		private readonly ILogger _auditLogger;
	}
}
