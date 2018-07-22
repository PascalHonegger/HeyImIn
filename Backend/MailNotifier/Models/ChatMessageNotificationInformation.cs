using System;

namespace HeyImIn.MailNotifier.Models
{
	public class ChatMessageNotificationInformation
	{
		public ChatMessageNotificationInformation(int authorId, DateTime sentDate, string content)
		{
			AuthorId = authorId;
			SentDate = sentDate;
			Content = content;
		}

		public int AuthorId { get; }

		public DateTime SentDate { get; }

		public string Content { get; }
	}
}
