using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventChatMessages
	{
		public EventChatMessages(List<EventChatMessage> messages, bool possiblyMoreMessages)
		{
			Messages = messages;
			PossiblyMoreMessages = possiblyMoreMessages;
		}

		public List<EventChatMessage> Messages { get; }

		public bool PossiblyMoreMessages { get; }
	}
}
