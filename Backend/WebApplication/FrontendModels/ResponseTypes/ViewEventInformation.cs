using System.Collections.Generic;
using System.Linq;
using HeyImIn.Database.Models;

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

		public static ViewEventInformation FromEvent(Event @event)
		{
			List<UserInformation> participants = @event.EventParticipations.Select(e => UserInformation.FromUserExcludingEmail(e.Participant)).ToList();

			UserInformation organizer = UserInformation.FromUserExcludingEmail(@event.Organizer);

			return new ViewEventInformation(@event.MeetingPlace, @event.Description, @event.IsPrivate, @event.Title, @event.SummaryTimeWindowInHours, @event.ReminderTimeWindowInHours, organizer, participants);
		}

		public UserInformation Organizer { get; }

		public List<UserInformation> Participants { get; }
	}
}
