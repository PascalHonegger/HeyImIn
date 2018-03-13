using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventDetails
	{
		public EventDetails(EventInformation information, List<AppointmentDetails> upcomingAppointments, NotificationConfiguration notificationConfiguration)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			NotificationConfiguration = notificationConfiguration;
		}

		public EventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public NotificationConfiguration NotificationConfiguration { get; }
	}
}
