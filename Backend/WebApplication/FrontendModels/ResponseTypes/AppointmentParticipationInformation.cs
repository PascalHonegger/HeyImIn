using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentParticipationInformation
	{
		public AppointmentParticipationInformation(string participantName, int participantId, AppointmentParticipationAnswer? response)
		{
			ParticipantName = participantName;
			ParticipantId = participantId;
			Response = response;
		}

		public string ParticipantName { get; }

		public int ParticipantId { get; }

		public AppointmentParticipationAnswer? Response { get; }
	}
}
