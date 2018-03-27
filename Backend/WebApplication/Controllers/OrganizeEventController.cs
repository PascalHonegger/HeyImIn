using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.WebApplication.FrontendModels;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class OrganizeEventController : Controller
	{
		public OrganizeEventController(INotificationService notificationService, IDeleteService deleteService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_deleteService = deleteService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Deletes an event and all underlying appointments
		///     Informs the users about the deletion
		/// </summary>
		/// <param name="eventId">
		///     <see cref="Event.Id" />
		/// </param>
		[HttpDelete]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> DeleteEvent(int eventId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
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
					_log.InfoFormat("{0}(): Tried to delete event {1}, which he's not organizing", nameof(DeleteEvent), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				EventNotificationInformation notificationInformation = _deleteService.DeleteEventLocally(context, @event);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted event {1}", nameof(DeleteEvent), @event.Id);

				await _notificationService.NotifyEventDeletedAsync(notificationInformation);

				return Ok();
			}
		}

		/// <summary>
		///     Updates general information about an event
		///     Informs the users about the change
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> UpdateEventInfo([FromBody] UpdatedEventInfoDto updatedEventInfoDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (updatedEventInfoDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
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
					_log.InfoFormat("{0}(): Tried to update event {1}, which he's not organizing", nameof(UpdateEventInfo), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				@event.Title = updatedEventInfoDto.Title;
				@event.MeetingPlace = updatedEventInfoDto.MeetingPlace;
				@event.Description = updatedEventInfoDto.Description;
				@event.IsPrivate = updatedEventInfoDto.IsPrivate;
				@event.ReminderTimeWindowInHours = updatedEventInfoDto.ReminderTimeWindowInHours;
				@event.SummaryTimeWindowInHours = updatedEventInfoDto.SummaryTimeWindowInHours;

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Updated event {1}", nameof(UpdateEventInfo), @event.Id);

				await _notificationService.NotifyEventUpdatedAsync(@event);

				return Ok();
			}
		}

		/// <summary>
		///     Creates a new event
		/// </summary>
		/// <returns>
		///     <see cref="Event.Id" />
		/// </returns>
		[HttpPost]
		[ProducesResponseType(typeof(int), 200)]
		public async Task<IActionResult> CreateEvent([FromBody] GeneralEventInformation generalEventInformation)
		{
			// Validate parameters
			if (!ModelState.IsValid || (generalEventInformation == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event newEvent = context.Events.Create();

				newEvent.Title = generalEventInformation.Title;
				newEvent.Description = generalEventInformation.Description;
				newEvent.MeetingPlace = generalEventInformation.MeetingPlace;
				newEvent.IsPrivate = generalEventInformation.IsPrivate;
				newEvent.ReminderTimeWindowInHours = generalEventInformation.ReminderTimeWindowInHours;
				newEvent.SummaryTimeWindowInHours = generalEventInformation.SummaryTimeWindowInHours;
				newEvent.OrganizerId = HttpContext.GetUserId();

				context.Events.Add(newEvent);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Created event {1} ({2})", nameof(CreateEvent), newEvent.Id, newEvent.Title);

				return Ok(newEvent.Id);
			}
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
		[HttpGet]
		[ProducesResponseType(typeof(EditEventDetails), 200)]
		public async Task<IActionResult> GetEditDetails(int eventId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.EventParticipations)
						.ThenInclude(ep => ep.Participant)
					.Include(e => e.Appointments)
						.ThenInclude(ap => ap.AppointmentParticipations)
					.Include(e => e.Organizer)
					.FirstOrDefaultAsync(e => e.Id == eventId);

				if (@event == null)
				{
					return NotFound();
				}

				int currentUserId = HttpContext.GetUserId();

				if (@event.OrganizerId != currentUserId)
				{
					_log.InfoFormat("{0}(): Tried to edit event {1}, which he's not organizing", nameof(GetEditDetails), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				List<User> allParticipants = @event.EventParticipations.Select(e => e.Participant).ToList();

				List<AppointmentDetails> upcomingAppointments = @event.Appointments
					.Where(a => a.StartTime >= DateTime.UtcNow)
					.OrderBy(a => a.StartTime)
					.Select(a => AppointmentDetails.FromAppointment(a, currentUserId, allParticipants))
					.ToList();

				List<EventParticipantInformation> currentEventParticipation = @event.EventParticipations.Select(EventParticipantInformation.FromParticipation).ToList();

				ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUserId);

				return Ok(new EditEventDetails(viewEventInformation, upcomingAppointments, currentEventParticipation));
			}
		}

		private readonly INotificationService _notificationService;
		private readonly IDeleteService _deleteService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
