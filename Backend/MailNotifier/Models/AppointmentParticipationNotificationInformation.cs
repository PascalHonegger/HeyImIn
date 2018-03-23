using HeyImIn.Database.Models;

namespace HeyImIn.MailNotifier.Models
{
	public class AppointmentParticipationNotificationInformation
	{
		public AppointmentParticipationNotificationInformation(UserNotificationInformation participant, AppointmentParticipationAnswer? answer, bool sentSummary, bool sentReminder)
		{
			Participant = participant;
			Answer = answer;
			SentSummary = sentSummary;
			SentReminder = sentReminder;
		}

		public UserNotificationInformation Participant { get; }
		public AppointmentParticipationAnswer? Answer { get; }
		public bool SentSummary { get; }
		public bool SentReminder { get; }
	}
}