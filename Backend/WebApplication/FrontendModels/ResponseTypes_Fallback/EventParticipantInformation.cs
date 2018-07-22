using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes_Fallback
{
	public class EventParticipantInformation
	{
		public EventParticipantInformation(int participantId, string participantName, string participantEmail)
		{
			ParticipantId = participantId;
			ParticipantName = participantName;
			ParticipantEmail = participantEmail;
		}

		public static EventParticipantInformation FromParticipation(EventParticipation participation)
		{
			return new EventParticipantInformation(participation.Participant.Id, participation.Participant.FullName, participation.Participant.Email);
		}

		public int ParticipantId { get; }

		public string ParticipantName { get; }

		public string ParticipantEmail { get; }
	}
}
