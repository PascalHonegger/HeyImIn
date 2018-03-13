using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class NotificationConfigurationResponse : NotificationConfiguration
	{
		private NotificationConfigurationResponse(bool sendReminderEmail, bool sendSummaryEmail, bool sendLastMinuteChangesEmail)
		{
			SendReminderEmail = sendReminderEmail;
			SendSummaryEmail = sendSummaryEmail;
			SendLastMinuteChangesEmail = sendLastMinuteChangesEmail;
		}

		public static NotificationConfigurationResponse FromParticipation(EventParticipation participation)
		{
			return new NotificationConfigurationResponse(participation.SendReminderEmail, participation.SendSummaryEmail, participation.SendLastMinuteChangesEmail);
		}
	}
}
