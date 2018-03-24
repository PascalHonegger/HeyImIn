using System;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using log4net;
using Microsoft.AspNetCore.Http;

namespace HeyImIn.WebApplication.Helpers
{
	public static class HttpActionExtensions
	{
		private const string UserIdPropertiesKey = "UserId";
		private const string SessionTokenPropertiesKey = "SessionToken";

		// Wrapper for SessionToken property
		public static Guid? TryGetSessionToken(this HttpContext requestMessage)
		{
			if (requestMessage.Items.TryGetValue(SessionTokenPropertiesKey, out object sessionToken))
			{
				return sessionToken as Guid?;
			}

			return null;
		}
		public static Guid GetSessionToken(this HttpContext requestMessage)
		{
			return (Guid)requestMessage.Items[SessionTokenPropertiesKey];
		}
		public static void SetSessionToken(this HttpContext requestMessage, Guid sessionToken)
		{
			requestMessage.Items[SessionTokenPropertiesKey] = sessionToken;
		}

		// Wrapper for UserId property
		public static int? TryGetUserId(this HttpContext requestMessage)
		{
			if (requestMessage.Items.TryGetValue(UserIdPropertiesKey, out object userId))
			{
				return userId as int?;
			}

			return null;
		}
		public static int GetUserId(this HttpContext requestMessage)
		{
			return 0; //(int)requestMessage.Items[UserIdPropertiesKey];
		}
		public static void SetUserId(this HttpContext requestMessage, int userId)
		{
			requestMessage.Items[UserIdPropertiesKey] = userId;
		}

		/// <summary>
		///     Loads the current user from the database using the provided <paramref name="context" />
		/// </summary>
		/// <param name="requestMessage">Request to get UserId from</param>
		/// <param name="context">Context to load from</param>
		/// <returns>Loaded user</returns>
		public static async Task<User> GetCurrentUserAsync(this HttpContext requestMessage, IDatabaseContext context)
		{
			int currentUserId = requestMessage.GetUserId();

			User currentUser = await context.Users.FindAsync(currentUserId);

			if (currentUser == null)
			{
				_log.ErrorFormat("{0}(): Couldn't load the current user from the database", nameof(GetCurrentUserAsync));
				throw new CurrentUserNotFoundException(currentUserId);
			}

			return currentUser;
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}