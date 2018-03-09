using log4net;

namespace HeyImIn.WebApplication.Helpers
{
	public static class LogHelpers
	{
		// Have to be in sync with log4net.config
		public const string UserIdPropertyKey = "UserId";
		public const string SessionTokenPropertyKey = "SessionToken";
		private const string AuditLoggerName = "AuditLogger";

		public static ILog GetAuditLog()
		{
			return LogManager.GetLogger(AuditLoggerName);
		}
	}
}