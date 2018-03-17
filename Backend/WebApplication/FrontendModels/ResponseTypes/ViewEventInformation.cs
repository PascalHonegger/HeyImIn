using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class ViewEventInformation : GeneralEventInformation
	{
		public ViewEventInformation(int organizerId, string organizerName, string meetingPlace, string description, int totalParticipants, bool isPrivate, bool currentUserDoesParticipate, string title, int summaryTimeWindowInHours, int reminderTimeWindowInHours)
		{
			OrganizerId = organizerId;
			OrganizerName = organizerName;
			MeetingPlace = meetingPlace;
			Description = description;
			TotalParticipants = totalParticipants;
			IsPrivate = isPrivate;
			CurrentUserDoesParticipate = currentUserDoesParticipate;
			Title = title;
			SummaryTimeWindowInHours = summaryTimeWindowInHours;
			ReminderTimeWindowInHours = reminderTimeWindowInHours;
		}

		public static ViewEventInformation FromEvent(Event @event, int currentUserId)
		{
			bool currentUserDoesParticipate = @event.EventParticipations.Select(p => p.ParticipantId).Contains(currentUserId);

			return new ViewEventInformation(@event.OrganizerId, @event.Organizer.FullName, @event.MeetingPlace, @event.Description, @event.EventParticipations.Count, @event.IsPrivate, currentUserDoesParticipate, @event.Title, @event.SummaryTimeWindowInHours, @event.ReminderTimeWindowInHours);
		}

		public int OrganizerId { get; }

		public string OrganizerName { get; }

		public int TotalParticipants { get; }

		public bool CurrentUserDoesParticipate { get; }
	}
}
