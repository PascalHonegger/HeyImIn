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
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class ParticipateEventController : ApiController
	{
		public ParticipateEventController(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Loads the <see cref="Event" />s the current user should see including the latest upcoming
		///     <see cref="Appointment" />
		/// </summary>
		/// <returns>
		///     <see cref="EventOverview" />
		/// </returns>
		[HttpGet]
		[ResponseType(typeof(EventOverview))]
		public async Task<IHttpActionResult> GetOverview()
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				List<Event> participatingEvents = currentUser.EventParticipations.Select(e => e.Event).ToList();
				List<Event> publicEvents = await context.Events.Where(e => !e.IsPrivate).Except(participatingEvents).ToListAsync();

				List<EventOverviewInformation> yourEventInformations = participatingEvents.Select(e => EventOverviewInformation.FromEvent(e, currentUser)).ToList();
				List<EventOverviewInformation> publicEventInformations = publicEvents.Select(e => EventOverviewInformation.FromEvent(e, currentUser)).ToList();

				return Ok(new EventOverview(yourEventInformations, publicEventInformations));
			}
		}

		/// <summary>
		///     Loads the <see cref="Event" /> for the provided <paramref name="id" /> and upcoming <see cref="Appointment" />
		/// </summary>
		/// <param name="id">
		///     <see cref="Event.Id" />
		/// </param>
		/// <returns>
		///     <see cref="EventDetails" />
		/// </returns>
		[HttpGet]
		[ResponseType(typeof(EventDetails))]
		public async Task<IHttpActionResult> GetDetails(int id)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.FindAsync(id);

				if (@event == null)
				{
					return NotFound();
				}

				List<User> allParticipants = @event.EventParticipations.Select(e => e.Participant).ToList();

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				List<AppointmentDetails> upcomingAppointments = @event.Appointments.Where(a => a.StartTime >= DateTime.UtcNow).OrderBy(a => a.StartTime).Take(ShownAppointmentsPerEvent).Select(a => AppointmentDetails.FromAppointment(a, currentUser, allParticipants)).ToList();

				EventParticipation currentEventParticipation = @event.EventParticipations.FirstOrDefault(e => e.Participant == currentUser);

				EventInformation eventInformation = EventInformation.FromEvent(@event, currentUser);

				NotificationConfiguration notificationConfiguration = null;

				if (currentEventParticipation != null)
				{
					notificationConfiguration = NotificationConfiguration.FromParticipation(currentEventParticipation);
				}

				return Ok(new EventDetails(eventInformation, upcomingAppointments, notificationConfiguration));
			}
		}

		/// <summary>
		///     Explizitly adds the current user to an event
		/// </summary>
		/// <param name="joinEventDto">
		///     <see cref="Event.Id" />
		/// </param>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> JoinEvent([FromBody] JoinEventDto joinEventDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (joinEventDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.FindAsync(joinEventDto.EventId);

				if (@event == null)
				{
					return NotFound();
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				JoinEventIfNotParticipating(@event, currentUser, context);

				await context.SaveChangesAsync();

				return Ok();
			}
		}

		/// <summary>
		///     Removes the specified user from an event. A user can remove himself any the <see cref="Event.Organizer" /> can
		///     remove any user
		/// </summary>
		/// <param name="leaveEventDto">
		///     <see cref="User.Id" />
		///     <see cref="Event.Id" />
		/// </param>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> LeaveEvent([FromBody] LeaveEventDto leaveEventDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (leaveEventDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.FindAsync(leaveEventDto.EventId);

				if (@event == null)
				{
					return BadRequest("Der angegebene Event wurde nicht gefunden");
				}

				User userToRemove = await context.Users.FindAsync(leaveEventDto.UserId);

				if (userToRemove == null)
				{
					return BadRequest("Der angegebene Benutzer wurde nicht gefunden");
				}

				EventParticipation participation = @event.EventParticipations.FirstOrDefault(e => e.Participant == userToRemove);

				if (participation == null)
				{
					return BadRequest("Der zu entfernende Benutzer nimmt nicht an diesem Event teil");
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				bool changingOtherUser = currentUser != userToRemove;

				if (changingOtherUser && (@event.Organizer != currentUser))
				{
					_log.InfoFormat("{0}(): Tried to remove user {1} from then event {2}, which he's not organizing", nameof(LeaveEvent), userToRemove.Id, @event.Id);

					return BadRequest("Sie organisieren diesen Event nicht");
				}

				context.EventParticipations.Remove(participation);

				List<AppointmentParticipation> appointmentParticipations = userToRemove.AppointmentParticipations.ToList();

				context.AppointmentParticipations.RemoveRange(appointmentParticipations);

				await context.SaveChangesAsync();

				// Handle notifications
				if (changingOtherUser)
				{
					_auditLog.InfoFormat("{0}(): The organizer removed the user {1} from the event {2}", nameof(LeaveEvent), userToRemove.Id, @event.Id);

					await _notificationService.NotifyOrganizerUpdatedUserInfoAsync(@event, userToRemove, "Der Organisator hat Sie vom Event entfernt");
				}
				else
				{
					_auditLog.InfoFormat("{0}(): Left the event {1}", nameof(LeaveEvent), @event.Id);
				}

				foreach (AppointmentParticipation appointmentParticipation in appointmentParticipations)
				{
					//TODO remove appointment => Possibly send notification
				}

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
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> SetAppointmentResponse([FromBody] SetAppointmentResponseDto setAppointmentResponseDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (setAppointmentResponseDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Appointment appointment = await context.Appointments.FindAsync(setAppointmentResponseDto.AppointmentId);

				if (appointment == null)
				{
					return BadRequest("Der angegebene Termin wurde nicht gefunden");
				}

				User userToSetResponseFor = await context.Users.FindAsync(setAppointmentResponseDto.UserId);

				if (userToSetResponseFor == null)
				{
					return BadRequest("Der angegebene Benutzer wurde nicht gefunden");
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				bool changingOtherUser = currentUser != userToSetResponseFor;

				if (changingOtherUser)
				{
					if (appointment.Event.Organizer != currentUser)
					{
						// Only the organizer is allowed to change another user
						_log.InfoFormat("{0}(): Tried to set response for user {1} for the appointment {2}, which he's not organizing", nameof(LeaveEvent), userToSetResponseFor.Id, appointment.Id);

						return BadRequest("Sie organisieren diesen Event nicht");
					}

					if (appointment.Event.EventParticipations.Select(e => e.Participant).Contains(userToSetResponseFor))
					{
						// A organizer shouldn't be able to add a user to the event, unless the user is participating in the event
						// This should prevent that a user gets added to an event he never had anything to do with
						return BadRequest("Der angegebene Benutzer nimmt nicht am Event teil");
					}
				}

				AppointmentParticipation participation = appointment.AppointmentParticipations.FirstOrDefault(e => e.Participant == userToSetResponseFor);

				if (participation == null)
				{
					participation = context.AppointmentParticipations.Create();
					participation.Participant = userToSetResponseFor;
					participation.Appointment = appointment;
					context.AppointmentParticipations.Add(participation);
				}

				if (setAppointmentResponseDto.Response.HasValue)
				{
					participation.AppointmentParticipationAnswer = setAppointmentResponseDto.Response.Value;
				}
				else
				{
					context.AppointmentParticipations.Remove(participation);
				}

				JoinEventIfNotParticipating(appointment.Event, userToSetResponseFor, context);

				await context.SaveChangesAsync();

				// Handle notifications
				if (changingOtherUser)
				{
					_auditLog.InfoFormat("{0}(response={1}): The organizer set the response to the appointment {2} for user {3}", nameof(LeaveEvent), setAppointmentResponseDto.Response, appointment.Id, userToSetResponseFor.Id);

					await _notificationService.NotifyOrganizerUpdatedUserInfoAsync(appointment.Event, userToSetResponseFor, $"Der Organisator hat Ihre Zusage vom Termin am {appointment.StartTime.ToHeyImInString()} editiert");
				}

				await _notificationService.SendLastMinuteChangeIfRequiredAsync(appointment);

				return Ok();
			}
		}

		/// <summary>
		///     Ensures the current user is participating in the event, not just the appointments
		/// </summary>
		private void JoinEventIfNotParticipating(Event @event, User currentUser, IDatabaseContext context)
		{
			if (@event.EventParticipations.Select(e => e.Participant).Contains(currentUser))
			{
				return;
			}

			EventParticipation participation = context.EventParticipations.Create();

			participation.Event = @event;
			participation.Participant = currentUser;

			context.EventParticipations.Add(participation);

			_auditLog.InfoFormat("{0}(): Joined event {1}", nameof(JoinEvent), @event.Id);
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		/// <summary>
		///     The maximum amount of appointments to show in the overview for each event
		///     In the edit view all events are shown
		/// </summary>
		private const int ShownAppointmentsPerEvent = 5;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}
