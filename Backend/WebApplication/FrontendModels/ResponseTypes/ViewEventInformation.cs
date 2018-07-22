using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class ViewEventInformation : GeneralEventInformation
	{
		public ViewEventInformation(string meetingPlace, string description, bool isPrivate, string title, int summaryTimeWindowInHours, int reminderTimeWindowInHours, UserInformation organizer, List<UserInformation> participants)
		{
			MeetingPlace = meetingPlace;
			Description = description;
			IsPrivate = isPrivate;
			Title = title;
			SummaryTimeWindowInHours = summaryTimeWindowInHours;
			ReminderTimeWindowInHours = reminderTimeWindowInHours;
			Organizer = organizer;
			Participants = participants;
		}

		public UserInformation Organizer { get; }

		public List<UserInformation> Participants { get; }
	}
}
