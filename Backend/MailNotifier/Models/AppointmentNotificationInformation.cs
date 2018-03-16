using System;
using System.Collections.Generic;

namespace HeyImIn.MailNotifier.Models
{
	public class AppointmentNotificationInformation
	{
		public AppointmentNotificationInformation(int id, DateTime startTime, List<AppointmentParticipationNotificationInformation> participations)
		{
			Id = id;
			StartTime = startTime;
			Participations = participations;
		}

		public int Id { get; }
		public DateTime StartTime { get; }
		public List<AppointmentParticipationNotificationInformation> Participations { get; }
	}
}