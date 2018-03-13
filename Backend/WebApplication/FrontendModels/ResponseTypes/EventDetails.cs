using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventDetails
	{
		public EventDetails(EventInformation information, List<AppointmentDetails> upcomingAppointments, NotificationConfigurationResponse notificationConfigurationResponse)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			NotificationConfigurationResponse = notificationConfigurationResponse;
		}

		public EventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public NotificationConfigurationResponse NotificationConfigurationResponse { get; }
	}
}
