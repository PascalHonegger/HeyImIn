namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class NotificationConfigurationResponse : NotificationConfiguration
	{
		public NotificationConfigurationResponse(bool sendReminderEmail, bool sendSummaryEmail, bool sendLastMinuteChangesEmail)
		{
			SendReminderEmail = sendReminderEmail;
			SendSummaryEmail = sendSummaryEmail;
			SendLastMinuteChangesEmail = sendLastMinuteChangesEmail;
		}
	}
}
