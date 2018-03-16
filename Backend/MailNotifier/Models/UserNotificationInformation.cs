namespace HeyImIn.MailNotifier.Models
{
	public class UserNotificationInformation
	{
		public UserNotificationInformation(int id, string fullName, string email)
		{
			Id = id;
			FullName = fullName;
			Email = email;
		}

		public int Id { get; }
		public string FullName { get; }
		public string Email { get; }
	}
}