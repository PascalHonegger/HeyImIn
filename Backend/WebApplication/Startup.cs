using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using HeyImIn.Authentication;
using HeyImIn.Authentication.Impl;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.External.DependencyInjection;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Impl;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.Services.Impl;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using log4net.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SendGrid;

namespace HeyImIn.WebApplication
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// ReSharper disable once UnusedMember.Global => This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc(options => { options.Filters.Add(new LogActionAttribute()); })
				.AddJsonOptions(options =>
				{
					// Frontend expects property names to be camelCase and vice versa
					options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
#if DEBUG
					// In debug pretty print the JSON
					options.SerializerSettings.Formatting = Formatting.Indented;
#endif
					options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
					// Convert all times to UTC
					options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
				});

			services.AddLogging();

			services.AddAuthorization(options => options.AddPolicy("RequiresLogin", policy => policy.AddRequirements(new RequiresLoginRequirement())));

#if DEBUG
			services.AddCors(options => options.AddPolicy("AllowFrontend", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new CorsAuthorizationFilterFactory("AllowFrontend"));
			});
#endif
		}

		// ReSharper disable once UnusedMember.Global => This method gets called by the runtime. Use this method to add services to the autofac container.
		public void ConfigureContainer(ContainerBuilder builder)
		{
			// Add future assemblies here to ensure the types can be resolved
			AssemblyScanner.Scan(builder,
				typeof(HomeController).Assembly,            // WebApplication
				typeof(IDatabaseContext).Assembly,          // Database
				typeof(INotificationService).Assembly,      // MailNotifier
				typeof(IAuthenticationService).Assembly     // Authentication
			);

			// Custom types with sensitive / global configuration
			string connectionString = _configuration.GetConnectionString("HeyImIn");
			var sendGridApiKey = _configuration.GetValue<string>("SENDGRID_API_KEY");

			// Other configuration values
			var baseWebUrl = _configuration.GetValue<string>("FrontendBaseUrl");
			var mailTimeZoneName = _configuration.GetValue<string>("MailTimeZoneName");

			int workFactor = _configuration.GetValue("PasswordHashWorkFactor", 10);

			// Register custom types
			builder.Register(c => new HeyImInDatabaseContext(connectionString)).As<IDatabaseContext>().InstancePerDependency();
			builder.Register(c => new SendGridClient(sendGridApiKey)).As<ISendGridClient>();
			builder.Register(c => new PasswordService(workFactor)).As<IPasswordService>();
			builder.Register(c => new NotificationService(c.Resolve<IMailSender>(), c.Resolve<ISessionService>(), baseWebUrl, mailTimeZoneName)).As<INotificationService>();
			builder.RegisterTypes(typeof(CronSendNotificationsService)).As<ICronService>(); // Add future cron services here

			// Register other types
			builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, IHostingEnvironment env, GetDatabaseContext contextFunc)
		{
			ConfigureLog4Net(env);

			using (IDatabaseContext context = contextFunc())
			{
				context.Migrate();
			}

			// TODO DatabaseConfiguration.ConfigureMigrations();

			lifetime.ApplicationStarted.Register(LogStarted);
			lifetime.ApplicationStopping.Register(LogStopping);
			lifetime.ApplicationStopped.Register(LogStopped);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc(routes => routes.MapRoute("default", "{controller=Home}/{action=Index}"));
		}

		private static void ConfigureLog4Net(IHostingEnvironment env)
		{
			// The mapping to the log4net.conf file is done through the AssemblyInfo.cs

			string logFileDirectory =
#if DEBUG
				Path.Combine(env.ContentRootPath, "App_Data");
#else
				"D:\\home\\LogFiles\\HeyImIn";
#endif

			if (!Directory.Exists(logFileDirectory))
			{
				// Make sure the directory exists
				Directory.CreateDirectory(logFileDirectory);
			}

			GlobalContext.Properties[LogHelpers.LogFileDirectoryKey] = logFileDirectory;

			// Hide (null) from logs => https://stackoverflow.com/a/22344774
			SystemInfo.NullText = string.Empty;

			_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		private static void LogStarted()
		{
			Version currentVersion = typeof(Startup).Assembly.GetName().Version;
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
			_log.InfoFormat("{0}(): {1} Started at version {2}", nameof(LogStarted), StartEndPrefix, currentVersion);
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
		}

		private static void LogStopping()
		{
			if (_log == null)
			{
				return;
			}

			_log.InfoFormat("{0}(): {1}", nameof(LogStopping), StartEndPrefix);
			_log.InfoFormat("{0}(): {1} Stopping...", nameof(LogStarted), StartEndPrefix);
			_log.InfoFormat("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
		}

		private static void LogStopped()
		{
			if (_log == null)
			{
				Trace.TraceError("No _log configured -> Startup probably failed");
				return;
			}

			_log.InfoFormat("{0}(): {1}", nameof(LogStopped), StartEndPrefix);
			_log.InfoFormat("{0}(): {1} Stopped", nameof(LogStopped), StartEndPrefix);
			_log.InfoFormat("{0}(): {1}", nameof(LogStopped), StartEndPrefix);
		}

		private readonly IConfiguration _configuration;

		private const string StartEndPrefix = ">>>>>>>>";
		private static ILog _log;
	}
}
