using System;

namespace HeyImIn.WebApplication.Helpers
{
	public class CurrentUserNotFoundException : Exception
	{
		public int UserId { get; }

		public CurrentUserNotFoundException(int userId)
		{
			UserId = userId;
		}
	}
}