using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentParticipationInformation
	{
		public AppointmentParticipationInformation(int participantId, AppointmentParticipationAnswer? response)
		{
			ParticipantId = participantId;
			Response = response;
		}

		public int ParticipantId { get; }

		public AppointmentParticipationAnswer? Response { get; }
	}
}
