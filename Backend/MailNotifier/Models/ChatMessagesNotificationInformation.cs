using System.Collections.Generic;

namespace HeyImIn.MailNotifier.Models
{
	public class ChatMessagesNotificationInformation
	{
		public ChatMessagesNotificationInformation(int eventId, string eventTitle, int participantId, List<ChatMessageNotificationInformation> messages, List<(int id, string fullName, string email)> relevantUserData)
		{
			EventId = eventId;
			EventTitle = eventTitle;
			ParticipantId = participantId;
			Messages = messages;
			RelevantUserData = relevantUserData;
		}

		public int EventId { get; }

		public string EventTitle { get; }

		public int ParticipantId { get; }

		public List<ChatMessageNotificationInformation> Messages { get; }

		public List<(int id, string fullName, string email)> RelevantUserData { get; }
	}
}
