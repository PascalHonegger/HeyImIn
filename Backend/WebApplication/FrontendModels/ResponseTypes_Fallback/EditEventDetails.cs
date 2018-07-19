using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes_Fallback
{
	public class EditEventDetails
	{
		public EditEventDetails(GeneralEventInformation information, List<AppointmentDetails> upcomingAppointments, List<EventParticipantInformation> participants)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			Participants = participants;
		}

		public GeneralEventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public List<EventParticipantInformation> Participants { get; }
	}
}
