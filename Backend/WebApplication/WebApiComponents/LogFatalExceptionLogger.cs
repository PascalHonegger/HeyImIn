using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using log4net;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	/// A global exception logger to write all exceptions to the log
	/// </summary>
	public class LogFatalExceptionLogger : IExceptionLogger
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
		{
			try
			{
				_log.FatalFormat("{0}(): Unhandled exception occured, ex={1}", nameof(LogAsync), context.Exception);
			}
			catch (Exception e)
			{
				Trace.TraceError($"{nameof(LogAsync)}(): Unhandled exception occured, ex={context.Exception}");
				Trace.TraceError($"{nameof(LogAsync)}(): Failed to write to _log, ex={e}");
			}

			return Task.CompletedTask;
		}
	}
}