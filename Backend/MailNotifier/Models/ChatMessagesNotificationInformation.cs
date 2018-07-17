using System.Collections.Generic;
using HeyImIn.Database.Models;

namespace HeyImIn.MailNotifier.Models
{
	public class ChatMessagesNotificationInformation
	{
		public ChatMessagesNotificationInformation(int eventId, string eventTitle, User participant, List<ChatMessageNotificationInformation> messages)
		{
			EventId = eventId;
			EventTitle = eventTitle;
			Participant = participant;
			Messages = messages;
		}

		public int EventId { get; }

		public string EventTitle { get; }

		public User Participant { get; }

		public List<ChatMessageNotificationInformation> Messages { get; }
	}
}
