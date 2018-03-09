using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using log4net.Config;
using log4net.Util;

namespace HeyImIn.WebApplication
{
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			ConfigureLog4Net();

			LogStarted();

			GlobalConfiguration.Configure(WebApiConfig.Register);

			GlobalConfiguration.Configuration.Services.Add(typeof(IExceptionLogger), new LogFatalExceptionLogger());
		}

		protected void Application_End()
		{
			LogStopped();
		}

		private void ConfigureLog4Net()
		{
			string logFileDirectory =
#if DEBUG
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
#else
				"D:\\Home\\LogFiles\\HeyImIn";
#endif

			if (!Directory.Exists(logFileDirectory))
			{
				// Make sure the directory exists
				Directory.CreateDirectory(logFileDirectory);
			}

			GlobalContext.Properties[LogHelpers.LogFileDirectoryKey] = logFileDirectory;

			// Hide (null) from logs => https://stackoverflow.com/a/22344774
			SystemInfo.NullText = string.Empty;

			// Expaned path to the configuration file
			string fullTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

			var fileInfo = new FileInfo(fullTemplatePath);

			if (fileInfo.Exists)
			{
				// Use XML configuration which can be edited on the fly
				XmlConfigurator.ConfigureAndWatch(fileInfo);
				SetLoggerInstance();
				_log.DebugFormat("{0}(): Loaded log4net configuration file from {1}", nameof(ConfigureLog4Net), fileInfo.FullName);
			}
			else
			{
				// Use log4net default configuration as a fallback
				BasicConfigurator.Configure();
				SetLoggerInstance();
				_log.ErrorFormat("{0}(): Couldn't find log4net configuration file at {1}", nameof(ConfigureLog4Net), fileInfo.FullName);
			}

			void SetLoggerInstance()
			{
				_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			}
		}

		private static void LogStarted()
		{
			Version currentVersion = typeof(WebApiApplication).Assembly.GetName().Version;
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
			_log.InfoFormat("{0}(): {1} Started at version {2}", nameof(LogStarted), StartEndPrefix, currentVersion);
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
		}

		private static void LogStopped()
		{
			if (_log == null)
			{
				Trace.TraceError("No _log configured -> Startup probably failed");
				return;
			}

			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
			_log.InfoFormat("{0}(): {1} Stopped", nameof(LogStarted), StartEndPrefix);
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
		}

		private const string StartEndPrefix = ">>>>>>>>";
		private static ILog _log;
	}
}
