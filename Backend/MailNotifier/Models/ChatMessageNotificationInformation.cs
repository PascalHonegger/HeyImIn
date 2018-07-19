using System;

namespace HeyImIn.MailNotifier.Models
{
	public class ChatMessageNotificationInformation
	{
		public ChatMessageNotificationInformation(string authorName, DateTime sentDate, string content)
		{
			AuthorName = authorName;
			SentDate = sentDate;
			Content = content;
		}

		public string AuthorName { get; }

		public DateTime SentDate { get; }

		public string Content { get; }
	}
}
