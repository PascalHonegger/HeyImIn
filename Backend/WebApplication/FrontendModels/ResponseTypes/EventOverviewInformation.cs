namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventOverviewInformation
	{
		public EventOverviewInformation(int eventId, ViewEventInformation viewEventInformation, AppointmentDetails? latestAppointmentDetails)
		{
			EventId = eventId;
			ViewEventInformation = viewEventInformation;
			LatestAppointmentDetails = latestAppointmentDetails;
		}

		public int EventId { get; }

		public ViewEventInformation ViewEventInformation { get; }

		public AppointmentDetails? LatestAppointmentDetails { get; }
	}
}
