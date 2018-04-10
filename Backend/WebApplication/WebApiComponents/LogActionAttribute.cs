using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HeyImIn.WebApplication.Helpers;
using log4net;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Sets context properties for the request and logs the completion
	/// </summary>
	public class LogActionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			// Save a stopwatch to measure the time it took to execute the request
			actionContext.Request.Properties[StopwatchName] = Stopwatch.StartNew();

			int? userId = actionContext.Request.TryGetUserId();
			if (userId.HasValue)
			{
				LogicalThreadContext.Properties[LogHelpers.UserIdLogKey] = userId.Value;
			}

			Guid? sessionToken = actionContext.Request.TryGetSessionToken();
			if (sessionToken.HasValue)
			{
				// Don't insert the full ID as that could be a security problem
				// E.g. User who can access the logs could then impersonate any user
				string semiUniqueId = sessionToken.Value.ToString("D").Substring(0, 8);
				LogicalThreadContext.Properties[LogHelpers.SessionTokenLogKey] = semiUniqueId;
			}

			base.OnActionExecuting(actionContext);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);
			var stopwatch = actionExecutedContext.Request.Properties[StopwatchName] as Stopwatch;

			string duration = stopwatch?.Elapsed.ToString("g") ?? "Unknown";
			string url = actionExecutedContext.Request.RequestUri.AbsolutePath;

			_log.DebugFormat("{0}(): WebApi method {1} returned, duration = {2}", nameof(OnActionExecuted), url, duration);

			LogicalThreadContext.Properties.Remove(LogHelpers.UserIdLogKey);
			LogicalThreadContext.Properties.Remove(LogHelpers.SessionTokenLogKey);
		}

		private const string StopwatchName = "RequestDurationStopwatch";
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
