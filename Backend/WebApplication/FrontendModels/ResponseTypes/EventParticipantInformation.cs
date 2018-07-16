using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class UserInformation
	{
		public UserInformation(int userId, string name, string email)
		{
			UserId = userId;
			Name = name;
			Email = email;
		}

		public static UserInformation FromUserIncludingEmail(User user)
		{
			return new UserInformation(user.Id, user.FullName, user.Email);
		}

		public static UserInformation FromUserExcludingEmail(User user)
		{
			return new UserInformation(user.Id, user.FullName, null);
		}

		public int UserId { get; }

		public string Name { get; }

		public string Email { get; }
	}
}
