namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class NotificationConfiguration
	{
		public NotificationConfiguration(bool sendReminderEmail, bool sendSummaryEmail, bool sendLastMinuteChangesEmail)
		{
			SendReminderEmail = sendReminderEmail;
			SendSummaryEmail = sendSummaryEmail;
			SendLastMinuteChangesEmail = sendLastMinuteChangesEmail;
		}

		public static NotificationConfiguration FromParticipation(Database.Models.EventParticipation participation)
		{
			return new NotificationConfiguration(participation.SendReminderEmail, participation.SendSummaryEmail, participation.SendLastMinuteChangesEmail);
		}

		public bool SendReminderEmail { get; }

		public bool SendSummaryEmail { get; }

		public bool SendLastMinuteChangesEmail { get; }
	}
}
