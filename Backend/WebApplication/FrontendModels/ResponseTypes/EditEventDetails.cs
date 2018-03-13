using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EditEventDetails
	{
		public EditEventDetails(EventInformation information, List<AppointmentDetails> upcomingAppointments, int reminderTimeWindowInHours, int summaryTimeWindowInHours, List<EventParticipationInformation> participants)
		{
			Information = information;
			UpcomingAppointments = upcomingAppointments;
			ReminderTimeWindowInHours = reminderTimeWindowInHours;
			SummaryTimeWindowInHours = summaryTimeWindowInHours;
			Participants = participants;
		}

		public EventInformation Information { get; }

		public List<AppointmentDetails> UpcomingAppointments { get; }

		public int ReminderTimeWindowInHours { get; }

		public int SummaryTimeWindowInHours { get; }

		public List<EventParticipationInformation> Participants { get; }
	}
}
