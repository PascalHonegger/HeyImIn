using System.Collections.Generic;

namespace HeyImIn.MailNotifier.Models
{
	public class EventNotificationInformation
	{
		public EventNotificationInformation(int id, string title, List<UserNotificationInformation> participations)
		{
			Id = id;
			Title = title;
			Participations = participations;
		}

		public int Id { get; }

		public string Title { get; }

		public List<UserNotificationInformation> Participations { get; }
	}
}
