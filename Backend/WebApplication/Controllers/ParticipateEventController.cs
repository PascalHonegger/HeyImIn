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
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using EventParticipation = HeyImIn.Database.Models.EventParticipation;

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
