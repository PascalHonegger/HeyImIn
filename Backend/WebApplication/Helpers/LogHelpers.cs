using log4net;

namespace HeyImIn.WebApplication.Helpers
{
	public static class LogHelpers
	{
		/// <summary>
		///     Returns the unique audit log to be used for important messages
		/// </summary>
		/// <returns></returns>
		public static ILog GetAuditLog()
		{
			return LogManager.GetLogger(AuditLoggerName);
		}

		// Have to be in sync with log4net.config
		public const string UserIdLogKey = "UserId";
		public const string SessionTokenLogKey = "SessionToken";
		public const string LogFileDirectoryKey = "LogFileDirectory";
		private const string AuditLoggerName = "AuditLogger";
	}
}
