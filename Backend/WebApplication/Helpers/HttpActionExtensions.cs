using System;
using System.Net.Http;

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
	}
}