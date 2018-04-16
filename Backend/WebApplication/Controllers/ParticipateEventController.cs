using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class ParticipateEventController : Controller
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
		[ProducesResponseType(typeof(EventOverview), 200)]
		public async Task<IActionResult> GetOverview()
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				int currentUserId = HttpContext.GetUserId();

				async Task<List<(Event @event, Appointment upcommingAppointment)>> GetAndFilterEvents(Expression<Func<Event, bool>> eventFilter)
				{
					List<Event> databaseResult = await context.Events
						.Where(eventFilter)
						.Include(e => e.EventParticipations)
						.Include(e => e.Organizer)
						.ToListAsync();

					var result = new List<(Event @event, Appointment upcommingAppointment)>();

					foreach (Event @event in databaseResult)
					{
						Appointment upcommingAppointment = await context.Entry(@event)
							.Collection(e => e.Appointments)
							.Query()
							.Include(a => a.AppointmentParticipations)
							.OrderBy(a => a.StartTime)
							.FirstOrDefaultAsync(a => a.StartTime >= DateTime.UtcNow);

						result.Add((@event, upcommingAppointment));
					}

					return result;
				}

				List<(Event @event, Appointment upcommingAppointment)> yourEvents = await GetAndFilterEvents(e => (e.OrganizerId == currentUserId) || e.EventParticipations.Select(ep => ep.ParticipantId).Contains(currentUserId));
				List<(Event @event, Appointment upcommingAppointment)> publicEvents = await GetAndFilterEvents(e => !e.IsPrivate && (e.OrganizerId != currentUserId) && !e.EventParticipations.Select(ep => ep.ParticipantId).Contains(currentUserId));

				List<EventOverviewInformation> yourEventInformations = yourEvents
					.Select(e => EventOverviewInformation.FromEvent(e.@event, e.upcommingAppointment, currentUserId))
					.OrderBy(e => e.LatestAppointmentInformation?.StartTime ?? DateTime.MaxValue)
					.ToList();
				List<EventOverviewInformation> publicEventInformations = publicEvents
					.Select(e => EventOverviewInformation.FromEvent(e.@event, e.upcommingAppointment, currentUserId))
					.OrderBy(e => e.LatestAppointmentInformation?.StartTime ?? DateTime.MaxValue)
					.ToList();

				return Ok(new EventOverview(yourEventInformations, publicEventInformations));
			}
		}

		/// <summary>
		///     Loads the <see cref="Event" /> for the provided <paramref name="eventId" /> and upcoming <see cref="Appointment" />
		/// </summary>
		/// <param name="eventId">
		///     <see cref="Event.Id" />
		/// </param>
		/// <returns>
		///     <see cref="EventDetails" />
		/// </returns>
		[HttpGet]
		[ProducesResponseType(typeof(EventDetails), 200)]
		public async Task<IActionResult> GetDetails(int eventId)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				var eventWithAppointments = await context.Events
					.Include(e => e.Organizer)
					.Include(e => e.EventParticipations)
						.ThenInclude(p => p.Participant)
					.Include(e => e.Appointments)
						.ThenInclude(a => a.AppointmentParticipations)
					.Select(e =>
						new
						{
							@event = e,
							appointments = e.Appointments
								.Where(a => a.StartTime >= DateTime.UtcNow)
								.OrderBy(a => a.StartTime)
								.Take(ShownAppointmentsPerEvent)
								.AsQueryable()
						})
					.FirstOrDefaultAsync(e => e.@event.Id == eventId);

				if (eventWithAppointments == null)
				{
					return NotFound();
				}

				Event @event = eventWithAppointments.@event;

				int currentUserId = HttpContext.GetUserId();

				ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUserId);

				if (!viewEventInformation.CurrentUserDoesParticipate && @event.IsPrivate && (@event.OrganizerId != currentUserId))
				{
					return BadRequest(RequestStringMessages.InvitationRequired);
				}

				List<User> allParticipants = @event.EventParticipations.Select(e => e.Participant).ToList();

				List<AppointmentDetails> upcomingAppointments = eventWithAppointments.appointments
					.Select(a => AppointmentDetails.FromAppointment(a, currentUserId, allParticipants))
					.ToList();

				EventParticipation currentEventParticipation = @event.EventParticipations.FirstOrDefault(e => e.ParticipantId == currentUserId);


				NotificationConfigurationResponse notificationConfigurationResponse = null;

				if (currentEventParticipation != null)
				{
					notificationConfigurationResponse = NotificationConfigurationResponse.FromParticipation(currentEventParticipation);
				}

				return Ok(new EventDetails(viewEventInformation, upcomingAppointments, notificationConfigurationResponse));
			}
		}

		/// <summary>
		///     Explicitly adds the current user to an event
		/// </summary>
		/// <param name="joinEventDto">
		///     <see cref="Event.Id" />
		/// </param>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> JoinEvent([FromBody] JoinEventDto joinEventDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (joinEventDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.EventParticipations)
					.FirstOrDefaultAsync(e => e.Id == joinEventDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				if (@event.EventParticipations.Any(e => e.Participant == currentUser))
				{
					return BadRequest(RequestStringMessages.UserAlreadyPartOfEvent);
				}

				if (@event.IsPrivate && (@event.Organizer != currentUser))
				{
					return BadRequest(RequestStringMessages.InvitationRequired);
				}

				EventParticipation eventParticipation = context.EventParticipations.Create();
				eventParticipation.Event = @event;
				eventParticipation.Participant = currentUser;

				context.EventParticipations.Add(eventParticipation);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Joined event {1}", nameof(JoinEvent), @event.Id);

				return Ok();
			}
		}

		/// <summary>
		///     Removes the specified user from an event. A user can remove himself and the <see cref="Event.Organizer" /> can
		///     remove any user
		/// </summary>
		/// <param name="removeFromEventDto">
		///     <see cref="User.Id" />
		///     <see cref="Event.Id" />
		/// </param>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> RemoveFromEvent([FromBody] RemoveFromEventDto removeFromEventDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (removeFromEventDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.FindAsync(removeFromEventDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User userToRemove = await context.Users.FindAsync(removeFromEventDto.UserId);

				if (userToRemove == null)
				{
					return BadRequest(RequestStringMessages.UserNotFound);
				}

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				bool changingOtherUser = currentUser != userToRemove;

				if (changingOtherUser && (@event.Organizer != currentUser))
				{
					_log.InfoFormat("{0}(): Tried to remove user {1} from then event {2}, which he's not organizing", nameof(RemoveFromEvent), userToRemove.Id, @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				EventParticipation participation = @event.EventParticipations.FirstOrDefault(e => e.Participant == userToRemove);

				if (participation == null)
				{
					return BadRequest(RequestStringMessages.UserNotPartOfEvent);
				}

				// Remove event participation
				context.EventParticipations.Remove(participation);

				// Remove appointment participations within the event
				List<AppointmentParticipation> appointmentParticipations = userToRemove.AppointmentParticipations.Where(a => a.Appointment.Event == @event).ToList();
				List<Appointment> appointments = appointmentParticipations.Select(a => a.Appointment).ToList();
				context.AppointmentParticipations.RemoveRange(appointmentParticipations);

				await context.SaveChangesAsync();

				// Handle notifications
				if (changingOtherUser)
				{
					_auditLog.InfoFormat("{0}(): The organizer removed the user {1} from the event {2}", nameof(RemoveFromEvent), userToRemove.Id, @event.Id);

					await _notificationService.NotifyOrganizerUpdatedUserInfoAsync(@event, userToRemove, "Der Organisator hat Sie vom Event entfernt.");
				}
				else
				{
					_auditLog.InfoFormat("{0}(): Left the event {1}", nameof(RemoveFromEvent), @event.Id);
				}

				foreach (Appointment appointment in appointments)
				{
					await _notificationService.SendLastMinuteChangeIfRequiredAsync(appointment);
				}

				return Ok();
			}
		}

		/// <summary>
		///     Configures the enabled notifications of the current user for the specified event
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> ConfigureNotifications([FromBody] NotificationConfigurationDto notificationConfigurationDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (notificationConfigurationDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events.FindAsync(notificationConfigurationDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				EventParticipation participation = @event.EventParticipations.FirstOrDefault(e => e.Participant == currentUser);

				if (participation == null)
				{
					return BadRequest(RequestStringMessages.UserNotPartOfEvent);
				}

				participation.SendReminderEmail = notificationConfigurationDto.SendReminderEmail;
				participation.SendSummaryEmail = notificationConfigurationDto.SendSummaryEmail;
				participation.SendLastMinuteChangesEmail = notificationConfigurationDto.SendLastMinuteChangesEmail;

				await context.SaveChangesAsync();

				_log.InfoFormat("{0}(): Updated notification settings", nameof(ConfigureNotifications));

				return Ok();
			}
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		/// <summary>
		///     The maximum amount of appointments to show in the overview for each event
		///     In the edit view all events are shown
		/// </summary>
		private const int ShownAppointmentsPerEvent = 5;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
