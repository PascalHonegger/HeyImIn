using Microsoft.Extensions.Logging;

namespace HeyImIn.Shared
{
	public static class LogHelpers
	{
		/// <summary>
		///     Returns the unique audit log to be used for important messages
		/// </summary>
		/// <returns></returns>
		public static ILogger CreateAuditLogger(this ILoggerFactory loggerFactory)
		{
			return loggerFactory.CreateLogger(AuditLoggerName);
		}

		// Have to be in sync with log4net.config
		public const string UserIdLogKey = "UserId";
		public const string SessionTokenLogKey = "SessionToken";
		public const string LogFileDirectoryKey = "LogFileDirectory";
		public const string AuditLoggerName = "AuditLogger";
	}
}
