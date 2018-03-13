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
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
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

				// Event itself
				context.Events.Remove(@event);

				// Participations for the event
				context.EventParticipations.RemoveRange(@event.EventParticipations);

				// Appointments of the event
				List<Appointment> appointments = @event.Appointments.ToList();
				context.Appointments.RemoveRange(appointments);
				context.AppointmentParticipations.RemoveRange(appointments.SelectMany(a => a.AppointmentParticipations));

				// Invitations to the event
				context.EventInvitations.RemoveRange(@event.EventInvitations);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted event {1}", nameof(DeleteEvent), @event.Id);

				await _notificationService.NotifyEventDeletedAsync(@event);

				return Ok();
			}
		}

		/// <summary>
		///     Deletes an event and all underlying appointments
		///     Informs the users about the deletion
		/// </summary>
		/// <param name="appointmentId">
		///     <see cref="Appointment.Id" />
		/// </param>
		[HttpDelete]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> DeleteAppointment(int appointmentId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Appointment appointment = await context.Appointments
					.Include(a => a.Event.Organizer)
					.Include(a => a.AppointmentParticipations)
					.FirstOrDefaultAsync(e => e.Id == appointmentId);

				if (appointment == null)
				{
					return BadRequest(RequestStringMessages.AppointmentNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (appointment.Event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to delete appointment {1}, which he's not organizing", nameof(DeleteEvent), appointment.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				// Appointment itself
				List<AppointmentParticipation> participations = appointment.AppointmentParticipations.ToList();
				context.Appointments.Remove(appointment);
				context.AppointmentParticipations.RemoveRange(participations);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted event {1}", nameof(DeleteEvent), appointment.Id);

				await _notificationService.NotifyAppointmentExplicitlyCanceledAsync(appointment);

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
		///     Adds new appointsments to the event
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> AddAppointments([FromBody] AddAppointsmentsDto addAppointsmentsDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (addAppointsmentsDto == null))
			{
				return BadRequest();
			}

			if (addAppointsmentsDto.StartTimes.Any(a => a < DateTime.UtcNow))
			{
				return BadRequest(RequestStringMessages.AppointmentsHaveToStartInTheFuture);
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == addAppointsmentsDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to add appointments to the event {1}, which he's not organizing", nameof(UpdateEventInfo), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				foreach (DateTime startTime in addAppointsmentsDto.StartTimes)
				{
					Appointment newAppointment = context.Appointments.Create();

					newAppointment.Event = @event;
					newAppointment.StartTime = startTime;

					context.Appointments.Add(newAppointment);
				}

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Added {1} to event {2}", nameof(UpdateEventInfo), addAppointsmentsDto.StartTimes.Count, @event.Id);

				return Ok();
			}
		}


		/// <summary>
		///     Adds new appointsments to the event
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> InviteParticipants([FromBody] InviteParticipantsDto inviteParticipantsDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (inviteParticipantsDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.Organizer)
					.Include(e => e.EventInvitations)
					.FirstOrDefaultAsync(e => e.Id == inviteParticipantsDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to add appointments to the event {1}, which he's not organizing", nameof(UpdateEventInfo), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				var mailInvites = new List<(string email, EventInvitation invite)>();

				foreach (string emailAddress in inviteParticipantsDto.EmailAddresses)
				{
					EventInvitation invite = context.EventInvitations.Create();
					invite.Requested = DateTime.UtcNow;
					invite.Event = @event;
					context.EventInvitations.Add(invite);

					mailInvites.Add((emailAddress, invite));
				}

				await context.SaveChangesAsync();

				await _notificationService.SendInvitationLinkAsync(mailInvites);

				return Ok();
			}
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}
