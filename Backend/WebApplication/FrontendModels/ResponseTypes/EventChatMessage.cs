using System;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventChatMessage
	{
		public EventChatMessage(int id, int authorId, string content, DateTime sentDate)
		{
			AuthorId = authorId;
			Content = content;
			SentDate = sentDate;
			Id = id;
		}

		public int Id { get; }
		public int AuthorId { get; }
		public string Content { get; }
		public DateTime SentDate { get; }
	}
}
