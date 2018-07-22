using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes_Fallback
{
	public class EventDetails
	{
		public EventDetails(ViewEventInformation information, List<AppointmentDetails> upcomingAppointments, NotificationConfigurationResponse notificationConfigurationResponse)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			NotificationConfigurationResponse = notificationConfigurationResponse;
		}

		public ViewEventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public NotificationConfigurationResponse NotificationConfigurationResponse { get; }
	}
}
