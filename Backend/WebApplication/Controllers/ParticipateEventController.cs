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

				// TODO Load relations for better performance
				List<Event> participatingEvents = currentUser.EventParticipations.Select(e => e.Event).ToList();
				List<Event> publicEvents = await context.Events.Where(e => !e.IsPrivate).Except(participatingEvents).ToListAsync();

				List<EventOverviewInformation> yourEventInformations = participatingEvents
					.Select(e => EventOverviewInformation.FromEvent(e, currentUser))
					.OrderBy(e => e.LatestAppointmentInformation?.StartTime)
					.ToList();
				List<EventOverviewInformation> publicEventInformations = publicEvents
					.Select(e => EventOverviewInformation.FromEvent(e, currentUser))
					.OrderBy(e => e.LatestAppointmentInformation?.StartTime)
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
		[ResponseType(typeof(EventDetails))]
		public async Task<IHttpActionResult> GetDetails(int eventId)
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

				List<User> allParticipants = @event.EventParticipations.Select(e => e.Participant).ToList();

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				List<AppointmentDetails> upcomingAppointments = @event.Appointments
					.Where(a => a.StartTime >= DateTime.UtcNow)
					.OrderBy(a => a.StartTime)
					.Take(ShownAppointmentsPerEvent)
					.Select(a => AppointmentDetails.FromAppointment(a, currentUser, allParticipants))
					.ToList();

				EventParticipation currentEventParticipation = @event.EventParticipations.FirstOrDefault(e => e.Participant == currentUser);

				ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUser);

				NotificationConfigurationResponse notificationConfigurationResponse = null;

				if (currentEventParticipation != null)
				{
					notificationConfigurationResponse = NotificationConfigurationResponse.FromParticipation(currentEventParticipation);
				}

				return Ok(new EventDetails(viewEventInformation, upcomingAppointments, notificationConfigurationResponse));
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
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				EventParticipation eventParticipation = context.EventParticipations.Create();
				eventParticipation.Event = @event;
				eventParticipation.Participant = currentUser;

				context.EventParticipations.Add(eventParticipation);

				_auditLog.InfoFormat("{0}(): Joined event {1}", nameof(JoinEvent), @event.Id);

				await context.SaveChangesAsync();

				return Ok();
			}
		}

		/// <summary>
		///     Removes the specified user from an event. A user can remove himself any the <see cref="Event.Organizer" /> can
		///     remove any user
		/// </summary>
		/// <param name="removeFromEventDto">
		///     <see cref="User.Id" />
		///     <see cref="Event.Id" />
		/// </param>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> RemoveFromEvent([FromBody] RemoveFromEventDto removeFromEventDto)
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

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

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

				foreach (Appointment appointment in appointmentParticipations.Select(a => a.Appointment))
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
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> ConfigureNotifications([FromBody] NotificationConfigurationDto notificationConfigurationDto)
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

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

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
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}
