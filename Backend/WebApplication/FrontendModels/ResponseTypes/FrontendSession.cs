using System;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class FrontendSession
	{
		public FrontendSession(Guid token, string fullName, string email)
		{
			Token = token;
			FullName = fullName;
			Email = email;
		}

		public Guid Token { get; }

		public string FullName { get; }

		public string Email { get; }
	}
}
