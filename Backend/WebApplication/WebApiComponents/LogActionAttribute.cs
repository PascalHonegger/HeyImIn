using System;
using System.Globalization;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Helpers;
using log4net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Sets context properties for the request and logs the completion
	/// </summary>
	public class LogActionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			int? userId = actionContext.HttpContext.TryGetUserId();
			if (userId.HasValue)
			{
				LogicalThreadContext.Properties[LogHelpers.UserIdLogKey] = userId.Value;
			}

			Guid? sessionToken = actionContext.HttpContext.TryGetSessionToken();
			if (sessionToken.HasValue)
			{
				// Don't insert the full ID as that could be a security problem
				// E.g. User who can access the logs could then impersonate any user
				string semiUniqueId = sessionToken.Value.ToString("D", CultureInfo.InvariantCulture).Substring(0, 8);
				LogicalThreadContext.Properties[LogHelpers.SessionTokenLogKey] = semiUniqueId;
			}
		}

		public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
		{
			LogicalThreadContext.Properties.Remove(LogHelpers.UserIdLogKey);
			LogicalThreadContext.Properties.Remove(LogHelpers.SessionTokenLogKey);
		}
	}
}
