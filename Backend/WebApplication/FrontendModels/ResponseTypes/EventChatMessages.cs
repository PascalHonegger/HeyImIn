using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventChatMessages
	{
		public EventChatMessages(List<EventChatMessage> messages, bool possiblyMoreMessages, int? lastMessageId)
		{
			Messages = messages;
			PossiblyMoreMessages = possiblyMoreMessages;
			LastMessageId = lastMessageId;
		}

		public List<EventChatMessage> Messages { get; }

		public bool PossiblyMoreMessages { get; }

		public int? LastMessageId { get; }
	}
}
