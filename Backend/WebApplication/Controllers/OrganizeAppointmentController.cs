using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class OrganizeAppointmentController : Controller
	{
		public OrganizeAppointmentController(INotificationService notificationService, IDeleteService deleteService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_deleteService = deleteService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Deletes an event and all underlying appointments
		///     Informs the users about the deletion
		/// </summary>
		/// <param name="appointmentId">
		///     <see cref="Appointment.Id" />
		/// </param>
		[HttpDelete]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> DeleteAppointment(int appointmentId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Appointment appointment = await context.Appointments
					.Include(a => a.Event)
					.Include(a => a.Event.Organizer)
					.Include(a => a.AppointmentParticipations)
					.FirstOrDefaultAsync(e => e.Id == appointmentId);

				if (appointment == null)
				{
					return BadRequest(RequestStringMessages.AppointmentNotFound);
				}

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				Event @event = appointment.Event;

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to delete appointment {1}, which he's not organizing", nameof(DeleteAppointment), appointment.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				AppointmentNotificationInformation notificationInformation = _deleteService.DeleteAppointmentLocally(context, appointment);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Canceled appointment {1}", nameof(DeleteAppointment), appointment.Id);

				await _notificationService.NotifyAppointmentExplicitlyCanceledAsync(notificationInformation, @event);

				return Ok();
			}
		}

		/// <summary>
		///     Adds new appointsments to the event
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> AddAppointments([FromBody] AddAppointsmentsDto addAppointsmentsDto)
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

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to add appointments to the event {1}, which he's not organizing", nameof(AddAppointments), @event.Id);

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

				_auditLog.InfoFormat("{0}(): Added {1} appointments to event {2}", nameof(AddAppointments), addAppointsmentsDto.StartTimes.Length, @event.Id);

				return Ok();
			}
		}

		/// <summary>
		///     Sets the response to an appointment
		///     Will add the user to the event if he's not part of it yet
		/// </summary>
		/// <param name="setAppointmentResponseDto">
		///     <see cref="User.Id" />
		///     <see cref="Appointment.Id" />
		/// </param>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> SetAppointmentResponse([FromBody] SetAppointmentResponseDto setAppointmentResponseDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (setAppointmentResponseDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Appointment appointment = await context.Appointments
					.Include(a => a.Event)
					.Include(a => a.Event.Organizer)
					.Include(a => a.Event.EventParticipations)
					.Include(a => a.AppointmentParticipations)
					.FirstOrDefaultAsync(a => a.Id == setAppointmentResponseDto.AppointmentId);

				if (appointment == null)
				{
					return BadRequest(RequestStringMessages.AppointmentNotFound);
				}

				User userToSetResponseFor = await context.Users.FindAsync(setAppointmentResponseDto.UserId);

				if (userToSetResponseFor == null)
				{
					return BadRequest(RequestStringMessages.UserNotFound);
				}

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				bool changingOtherUser = currentUser != userToSetResponseFor;

				bool userIsPartOfEvent = appointment.Event.EventParticipations.Select(e => e.Participant).Contains(userToSetResponseFor);

				if (changingOtherUser)
				{
					if (appointment.Event.Organizer != currentUser)
					{
						// Only the organizer is allowed to change another user
						_log.InfoFormat("{0}(): Tried to set response for user {1} for the appointment {2}, which he's not organizing", nameof(SetAppointmentResponse), userToSetResponseFor.Id, appointment.Id);

						return BadRequest(RequestStringMessages.OrganizorRequired);
					}

					if (!userIsPartOfEvent)
					{
						// A organizer shouldn't be able to add a user to the event, unless the user is participating in the event
						// This should prevent that a user gets added to an event he never had anything to do with
						return BadRequest(RequestStringMessages.UserNotPartOfEvent);
					}
				}

				// Ensure a user can't participate in a private event without an invitation
				if (!userIsPartOfEvent && appointment.Event.IsPrivate && (appointment.Event.Organizer != currentUser))
				{
					return BadRequest(RequestStringMessages.InvitationRequired);
				}

				AppointmentParticipation appointmentParticipation = appointment.AppointmentParticipations.FirstOrDefault(e => e.Participant == userToSetResponseFor);

				if (appointmentParticipation == null)
				{
					// Create a new participation if there isn't an existing one
					appointmentParticipation = context.AppointmentParticipations.Create();
					appointmentParticipation.Participant = userToSetResponseFor;
					appointmentParticipation.Appointment = appointment;
					context.AppointmentParticipations.Add(appointmentParticipation);
				}

				if (setAppointmentResponseDto.Response.HasValue)
				{
					appointmentParticipation.AppointmentParticipationAnswer = setAppointmentResponseDto.Response.Value;
				}
				else
				{
					context.AppointmentParticipations.Remove(appointmentParticipation);
				}

				if (!appointment.Event.EventParticipations.Select(e => e.Participant).Contains(currentUser))
				{
					// Automatically add a user to an event if he's not yet part of it
					EventParticipation eventParticipation = context.EventParticipations.Create();
					eventParticipation.Event = appointment.Event;
					eventParticipation.Participant = currentUser;

					context.EventParticipations.Add(eventParticipation);

					_auditLog.InfoFormat("{0}(): Joined event {1} automatically after joining appointment {2}", nameof(SetAppointmentResponse), appointment.Event.Id, appointment.Id);
				}

				await context.SaveChangesAsync();

				// Handle notifications
				if (changingOtherUser)
				{
					_auditLog.InfoFormat("{0}(response={1}): The organizer set the response to the appointment {2} for user {3}", nameof(SetAppointmentResponse), setAppointmentResponseDto.Response, appointment.Id, userToSetResponseFor.Id);

					await _notificationService.NotifyOrganizerUpdatedUserInfoAsync(appointment.Event, userToSetResponseFor, "Der Organisator hat Ihre Zusage an einem Termin editiert.");
				}
				else
				{
					_log.DebugFormat("{0}(response={1}): Set own response for appointment {2}", nameof(SetAppointmentResponse), setAppointmentResponseDto.Response, appointment.Id);
				}

				await _notificationService.SendLastMinuteChangeIfRequiredAsync(appointment);

				return Ok();
			}
		}

		private readonly INotificationService _notificationService;
		private readonly IDeleteService _deleteService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
