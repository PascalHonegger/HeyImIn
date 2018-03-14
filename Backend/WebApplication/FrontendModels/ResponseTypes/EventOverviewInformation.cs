using System;
using System.Linq;
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

		public static EventOverviewInformation FromEvent(Event @event, User currentUser)
		{
			ViewEventInformation viewEventInformation = ViewEventInformation.FromEvent(@event, currentUser);

			Appointment firstUpcomingAppointment = @event.Appointments.FirstOrDefault(a => a.StartTime >= DateTime.UtcNow);

			if (firstUpcomingAppointment == null)
			{
				return new EventOverviewInformation(@event.Id, viewEventInformation, null);
			}

			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(firstUpcomingAppointment, currentUser, @event.EventParticipations.Count);

			return new EventOverviewInformation(@event.Id, viewEventInformation, appointmentInformation);
		}

		public int EventId { get; }

		public ViewEventInformation ViewEventInformation { get; }

		public AppointmentInformation LatestAppointmentInformation { get; }
	}
}
