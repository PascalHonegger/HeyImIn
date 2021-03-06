﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.Shared;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
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
	[ApiVersion(ApiVersions.Version2_0)]
	[Route("api/OrganizeAppointment")]
	public class OrganizeAppointmentController : ControllerBase
	{
		public OrganizeAppointmentController(INotificationService notificationService, IDeleteService deleteService, GetDatabaseContext getDatabaseContext, ILogger<OrganizeAppointmentController> logger, ILoggerFactory loggerFactory)
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
		/// <param name="appointmentId">
		///     <see cref="Appointment.Id" />
		/// </param>
		[HttpDelete(nameof(DeleteAppointment))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> DeleteAppointment(int appointmentId)
		{
			IDatabaseContext context = _getDatabaseContext();
			Appointment? appointment = await context.Appointments
				.Include(a => a.Event)
					.ThenInclude(e => e.Organizer)
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
				_logger.LogInformation("{0}(): Tried to delete appointment {1}, which he's not organizing", nameof(DeleteAppointment), appointment.Id);

				return BadRequest(RequestStringMessages.OrganizerRequired);
			}

			AppointmentNotificationInformation notificationInformation = _deleteService.DeleteAppointmentLocally(context, appointment);

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Canceled appointment {1}", nameof(DeleteAppointment), appointment.Id);

			await _notificationService.NotifyAppointmentExplicitlyCanceledAsync(notificationInformation, @event);

			return Ok();
		}

		/// <summary>
		///     Adds new appointments to the event
		/// </summary>
		[HttpPost(nameof(AddAppointments))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> AddAppointments(AddAppointmentsDto addAppointmentsDto)
		{
			if (addAppointmentsDto.StartTimes.Any(a => a < DateTime.UtcNow))
			{
				return BadRequest(RequestStringMessages.AppointmentsHaveToStartInTheFuture);
			}

			IDatabaseContext context = _getDatabaseContext();
			Event? @event = await context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == addAppointmentsDto.EventId);

			if (@event == null)
			{
				return BadRequest(RequestStringMessages.EventNotFound);
			}

			User currentUser = await HttpContext.GetCurrentUserAsync(context);

			if (@event.Organizer != currentUser)
			{
				_logger.LogInformation("{0}(): Tried to add appointments to the event {1}, which he's not organizing", nameof(AddAppointments), @event.Id);

				return BadRequest(RequestStringMessages.OrganizerRequired);
			}

			foreach (DateTime startTime in addAppointmentsDto.StartTimes)
			{
				var newAppointment = new Appointment
				{
					Event = @event,
					StartTime = startTime
				};


				context.Appointments.Add(newAppointment);
			}

			await context.SaveChangesAsync();

			_auditLogger.LogInformation("{0}(): Added {1} appointments to event {2}", nameof(AddAppointments), addAppointmentsDto.StartTimes.Length, @event.Id);

			return Ok();
		}

		/// <summary>
		///     Sets the response to an appointment
		///     Will add the user to the event if he's not part of it yet
		/// </summary>
		/// <param name="setAppointmentResponseDto">
		///     <see cref="User.Id" />
		///     <see cref="Appointment.Id" />
		/// </param>
		[HttpPost(nameof(SetAppointmentResponse))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> SetAppointmentResponse(SetAppointmentResponseDto setAppointmentResponseDto)
		{
			IDatabaseContext context = _getDatabaseContext();
			Appointment? appointment = await context.Appointments
				.Include(a => a.Event)
					.ThenInclude(e => e.Organizer)
				.Include(a => a.Event)
					.ThenInclude(e => e.EventParticipations)
				.Include(a => a.AppointmentParticipations)
					.ThenInclude(ap => ap.Participant)
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

			bool userIsPartOfEvent = appointment.Event.EventParticipations.Any(e => e.ParticipantId == userToSetResponseFor.Id);

			if (changingOtherUser)
			{
				if (appointment.Event.Organizer != currentUser)
				{
					// Only the organizer is allowed to change another user
					_logger.LogInformation("{0}(): Tried to set response for user {1} for the appointment {2}, which he's not organizing", nameof(SetAppointmentResponse), userToSetResponseFor.Id, appointment.Id);

					return BadRequest(RequestStringMessages.OrganizerRequired);
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

			AppointmentParticipation appointmentParticipation = appointment.AppointmentParticipations.FirstOrDefault(e => e.ParticipantId == userToSetResponseFor.Id);

			if (appointmentParticipation == null)
			{
				// Create a new participation if there isn't an existing one
				appointmentParticipation = new AppointmentParticipation
				{
					Participant = userToSetResponseFor,
					Appointment = appointment
				};

				context.AppointmentParticipations.Add(appointmentParticipation);
			}

			bool changeRelevantForSummary =
				(appointmentParticipation.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted) ||
				(setAppointmentResponseDto.Response == AppointmentParticipationAnswer.Accepted);

			appointmentParticipation.AppointmentParticipationAnswer = setAppointmentResponseDto.Response;

			if (!appointment.Event.EventParticipations.Select(e => e.ParticipantId).Contains(currentUser.Id))
			{
				// Automatically add a user to an event if he's not yet part of it
				var eventParticipation = new EventParticipation
				{
					Event = appointment.Event,
					Participant = currentUser
				};

				context.EventParticipations.Add(eventParticipation);

				_auditLogger.LogInformation("{0}(): Joined event {1} automatically after joining appointment {2}", nameof(SetAppointmentResponse), appointment.Event.Id, appointment.Id);
			}

			await context.SaveChangesAsync();

			// Handle notifications
			if (changingOtherUser)
			{
				_auditLogger.LogInformation("{0}(response={1}): The organizer set the response to the appointment {2} for user {3}", nameof(SetAppointmentResponse), setAppointmentResponseDto.Response, appointment.Id, userToSetResponseFor.Id);

				await _notificationService.NotifyOrganizerUpdatedUserInfoAsync(appointment.Event, userToSetResponseFor, "Der Organisator hat Ihre Zusage an einem Termin editiert.");
			}
			else
			{
				_logger.LogDebug("{0}(response={1}): Set own response for appointment {2}", nameof(SetAppointmentResponse), setAppointmentResponseDto.Response, appointment.Id);
			}

			if (changeRelevantForSummary)
			{
				// Don't send a last minute change if the response change is irrelevant
				// E.g. Changing from No Answer to Declined
				await _notificationService.SendLastMinuteChangeIfRequiredAsync(appointment);
			}
			else
			{
				_logger.LogDebug("{0}(response={1}): Intentionally skipped last minute change as the change wasn't relevant for the summary", nameof(SetAppointmentResponse), setAppointmentResponseDto.Response, appointment.Id);
			}

			return Ok();
		}

		private readonly INotificationService _notificationService;
		private readonly IDeleteService _deleteService;
		private readonly GetDatabaseContext _getDatabaseContext;
		private readonly ILogger<OrganizeAppointmentController> _logger;
		private readonly ILogger _auditLogger;
	}
}
