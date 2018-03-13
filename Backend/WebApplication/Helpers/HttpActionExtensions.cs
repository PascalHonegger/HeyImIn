using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using log4net;

namespace HeyImIn.WebApplication.Helpers
{
	public static class HttpActionExtensions
	{
		private const string UserIdPropertiesKey = "UserId";
		private const string SessionTokenPropertiesKey = "SessionToken";

		// Wrapper for SessionToken property
		public static Guid? TryGetSessionToken(this HttpRequestMessage requestMessage)
		{
			if (requestMessage.Properties.TryGetValue(SessionTokenPropertiesKey, out object sessionToken))
			{
				return sessionToken as Guid?;
			}

			return null;
		}
		public static Guid GetSessionToken(this HttpRequestMessage requestMessage)
		{
			return (Guid)requestMessage.Properties[SessionTokenPropertiesKey];
		}
		public static void SetSessionToken(this HttpRequestMessage requestMessage, Guid sessionToken)
		{
			requestMessage.Properties[UserIdPropertiesKey] = sessionToken;
		}

		// Wrapper for UserId property
		public static int? TryGetUserId(this HttpRequestMessage requestMessage)
		{
			if (requestMessage.Properties.TryGetValue(UserIdPropertiesKey, out object userId))
			{
				return userId as int?;
			}

			return null;
		}
		public static int GetUserId(this HttpRequestMessage requestMessage)
		{
			return (int)requestMessage.Properties[UserIdPropertiesKey];
		}
		public static void SetUserId(this HttpRequestMessage requestMessage, int userId)
		{
			requestMessage.Properties[UserIdPropertiesKey] = userId;
		}

		public static async Task<User> GetCurrentUserAsync(this HttpRequestMessage requestMessage, IDatabaseContext context)
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