using System;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventChatMessage
	{
		public EventChatMessage(int id, string authorName, string content, DateTime sentDate)
		{
			AuthorName = authorName;
			Content = content;
			SentDate = sentDate;
			Id = id;
		}

		public int Id { get; }
		public string AuthorName { get; } // TODO Keep redundant information to a minimum
		public string Content { get; }
		public DateTime SentDate { get; }
	}
}
