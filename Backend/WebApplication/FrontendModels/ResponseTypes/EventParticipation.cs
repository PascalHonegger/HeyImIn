﻿namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventParticipation
	{
		public EventParticipation(int participantId, string participantName, string participantEmail)
		{
			ParticipantId = participantId;
			ParticipantName = participantName;
			ParticipantEmail = participantEmail;
		}

		public int ParticipantId { get; }

		public string ParticipantName { get; }

		public string ParticipantEmail { get; }
	}
}
