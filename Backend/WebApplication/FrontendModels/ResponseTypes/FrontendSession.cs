using System;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class FrontendSession
	{
		public FrontendSession(Guid token, int userId, string fullName, string email)
		{
			Token = token;
			UserId = userId;
			FullName = fullName;
			Email = email;
		}

		public Guid Token { get; }

		public int UserId { get; }

		public string FullName { get; }

		public string Email { get; }
	}
}
