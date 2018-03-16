using System;

namespace HeyImIn.WebApplication.Helpers
{
	/// <summary>
	///     Exception used if the current user could not be found even though he should exist
	/// </summary>
	public class CurrentUserNotFoundException : Exception
	{
		public CurrentUserNotFoundException(int userId)
		{
			UserId = userId;
		}

		public int UserId { get; }
	}
}
