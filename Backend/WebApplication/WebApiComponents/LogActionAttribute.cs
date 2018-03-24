using System;
using System.Diagnostics;
using System.Reflection;
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
			// Save a stopwatch to measure the time it took to execute the request
			actionContext.HttpContext.Items[StopwatchName] = Stopwatch.StartNew();

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
				string semiUniqueId = sessionToken.Value.ToString("D").Substring(0, 8);
				LogicalThreadContext.Properties[LogHelpers.SessionTokenLogKey] = semiUniqueId;
			}

			base.OnActionExecuting(actionContext);
		}

		public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);
			var stopwatch = actionExecutedContext.HttpContext.Items[StopwatchName] as Stopwatch;

			string duration = stopwatch?.Elapsed.ToString("g") ?? "Unknown";
			string method = actionExecutedContext.HttpContext.Request.Method;
			string path = actionExecutedContext.HttpContext.Request.Path;

			_log.DebugFormat("{0}(): Finished {1} {2} in {3}", nameof(OnActionExecuted), method, path, duration);

			LogicalThreadContext.Properties.Remove(LogHelpers.UserIdLogKey);
			LogicalThreadContext.Properties.Remove(LogHelpers.SessionTokenLogKey);
		}

		private const string StopwatchName = "RequestDurationStopwatch";
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
