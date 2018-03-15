using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class OrganizeEventController : ApiController
	{
		public OrganizeEventController(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
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
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> DeleteEvent(int eventId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.Organizer)
					.Include(e => e.EventInvitations)
					.Include(e => e.EventParticipations)
					.Include(e => e.Appointments)
					.Include(e => e.Appointments.Select(a => a.AppointmentParticipations))
					.FirstOrDefaultAsync(e => e.Id == eventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to delete event {1}, which he's not organizing", nameof(DeleteEvent), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				// Participations for the event
				List<EventParticipation> participations = @event.EventParticipations.ToList();
				context.EventParticipations.RemoveRange(participations);

				// Appointments of the event
				List<Appointment> appointments = @event.Appointments.ToList();
				context.Appointments.RemoveRange(appointments);
				context.AppointmentParticipations.RemoveRange(appointments.SelectMany(a => a.AppointmentParticipations));

				// Invitations to the event
				context.EventInvitations.RemoveRange(@event.EventInvitations);

				// Event itself
				context.Events.Remove(@event);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted event {1}", nameof(DeleteEvent), @event.Id);

				await _notificationService.NotifyEventDeletedAsync(@event.Title, participations);

				return Ok();
			}
		}

		/// <summary>
		///     Updates general information about an event
		///     Informs the users about the change
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> UpdateEventInfo([FromBody] UpdatedEventInfoDto updatedEventInfoDto)
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

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

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
		[ResponseType(typeof(int))]
		public async Task<IHttpActionResult> CreateEvent([FromBody] GeneralEventInformation generalEventInformation)
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
				newEvent.OrganizerId = ActionContext.Request.GetUserId();

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
		[ResponseType(typeof(EditEventDetails))]
		public async Task<IHttpActionResult> GetEditDetails(int eventId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.Organizer)
					.Include(e => e.EventParticipations)
					.Include(e => e.Appointments)
					.FirstOrDefaultAsync(e => e.Id == eventId);

				if (@event == null)
				{
					return NotFound();
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to edit event {1}, which he's not organizing", nameof(GetEditDetails), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				List<User> allParticipants = @event.EventParticipations.Select(e => e.Participant).ToList();

				List<AppointmentDetails> upcomingAppointments = @event.Appointments
					.Where(a => a.StartTime >= DateTime.UtcNow)
					.OrderBy(a => a.StartTime)
					.Select(a => AppointmentDetails.FromAppointment(a, currentUser, allParticipants))
					.ToList();

				List<EventParticipantInformation> currentEventParticipation = @event.EventParticipations.Select(EventParticipantInformation.FromParticipation).ToList();

				ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUser);

				return Ok(new EditEventDetails(viewEventInformation, upcomingAppointments, currentEventParticipation));
			}
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}
