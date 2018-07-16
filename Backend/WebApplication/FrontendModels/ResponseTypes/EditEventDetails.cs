using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EditEventDetails
	{
		public EditEventDetails(GeneralEventInformation information, List<AppointmentDetails> upcomingAppointments, List<UserInformation> participants)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			Participants = participants;
		}

		public GeneralEventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public List<UserInformation> Participants { get; }
	}
}
