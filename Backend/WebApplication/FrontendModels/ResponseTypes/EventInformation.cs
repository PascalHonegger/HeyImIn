using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventInformation
	{
		public EventInformation(int eventId, int organizerId, string organizerName, string meetingPlace, string description, int totalParticipants, bool isPrivate, bool currentUserDoesParticipate)
		{
			EventId = eventId;
			OrganizerId = organizerId;
			OrganizerName = organizerName;
			MeetingPlace = meetingPlace;
			Description = description;
			TotalParticipants = totalParticipants;
			IsPrivate = isPrivate;
			CurrentUserDoesParticipate = currentUserDoesParticipate;
		}

		public static EventInformation FromEvent(Event @event, User currentUser)
		{
			bool currentUserDoesParticipate = @event.EventParticipations.Select(p => p.Participant).Contains(currentUser);

			return new EventInformation(@event.Id, @event.OrganizerId, @event.Organizer.FullName, @event.MeetingPlace, @event.Description, @event.EventParticipations.Count, @event.IsPrivate, currentUserDoesParticipate);
		}

		public int EventId { get; }

		public int OrganizerId { get; }

		public string OrganizerName { get; }

		public string MeetingPlace { get; }

		public string Description { get; }

		public bool IsPrivate { get; }

		public int TotalParticipants { get; }

		public bool CurrentUserDoesParticipate { get; }
	}
}
