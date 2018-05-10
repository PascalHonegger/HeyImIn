using System;
using log4net;

namespace HeyImIn.WebApplication.Helpers
{
	public static class LogHelpers
	{
		/// <summary>
		///     Returns the unique audit log to be used for important messages
		/// </summary>
		/// <returns></returns>
		public static ILog GetAuditLog(Type type)
		{
			return LogManager.GetLogger(type); // TODO
		}

		// Have to be in sync with log4net.config
		public const string UserIdLogKey = "UserId";
		public const string SessionTokenLogKey = "SessionToken";
		public const string LogFileDirectoryKey = "LogFileDirectory";
		// TODO private const string AuditLoggerName = "AuditLogger";
	}
}
