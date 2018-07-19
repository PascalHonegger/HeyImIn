using System;
using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentDetails
	{
		public AppointmentDetails(int appointmentId, DateTime startTime, List<AppointmentParticipationInformation> participations)
		{
			AppointmentId = appointmentId;
			StartTime = startTime;
			Participations = participations;
		}

		public int AppointmentId { get; }

		public DateTime StartTime { get; }

		public List<AppointmentParticipationInformation> Participations { get; }
	}
}
