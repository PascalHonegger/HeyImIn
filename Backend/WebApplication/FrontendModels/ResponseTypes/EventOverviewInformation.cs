using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventOverviewInformation
	{
		public EventOverviewInformation(int eventId, ViewEventInformation viewEventInformation, AppointmentInformation latestAppointmentInformation)
		{
			EventId = eventId;
			ViewEventInformation = viewEventInformation;
			LatestAppointmentInformation = latestAppointmentInformation;
		}

		public static EventOverviewInformation FromEvent(Event @event, Appointment firstUpcomingAppointment, int currentUserId)
		{
			ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUserId);

			if (firstUpcomingAppointment == null)
			{
				return new EventOverviewInformation(@event.Id, viewEventInformation, null);
			}

			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(firstUpcomingAppointment, currentUserId, @event.EventParticipations.Count);

			return new EventOverviewInformation(@event.Id, viewEventInformation, appointmentInformation);
		}

		public int EventId { get; }

		public ViewEventInformation ViewEventInformation { get; }

		public AppointmentInformation LatestAppointmentInformation { get; }
	}
}
