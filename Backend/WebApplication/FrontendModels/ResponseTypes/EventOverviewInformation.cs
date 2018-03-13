using System;
using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventOverviewInformation
	{
		public EventOverviewInformation(EventInformation eventInformation, AppointmentInformation latestAppointmentInformation)
		{
			EventInformation = eventInformation;
			LatestAppointmentInformation = latestAppointmentInformation;
		}

		public static EventOverviewInformation FromEvent(Event @event, User currentUser)
		{
			EventInformation eventInformation = EventInformation.FromEvent(@event, currentUser);

			Appointment firstUpcomingAppointment = @event.Appointments.FirstOrDefault(a => a.StartTime >= DateTime.UtcNow);

			if (firstUpcomingAppointment == null)
			{
				return new EventOverviewInformation(eventInformation, null);
			}

			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(firstUpcomingAppointment, currentUser, @event.EventParticipations.Count);

			return new EventOverviewInformation(eventInformation, appointmentInformation);
		}

		public EventInformation EventInformation { get; }

		public AppointmentInformation LatestAppointmentInformation { get; }
	}
}
