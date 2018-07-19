using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventChatMessages
	{
		public EventChatMessages(List<EventChatMessage> messages, bool possiblyMoreMessages, List<UserInformation> authorInformations)
		{
			Messages = messages;
			PossiblyMoreMessages = possiblyMoreMessages;
			AuthorInformations = authorInformations;
		}

		public List<EventChatMessage> Messages { get; }

		public bool PossiblyMoreMessages { get; }

		public List<UserInformation> AuthorInformations { get; }
	}
}
