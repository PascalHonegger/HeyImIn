using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.MailNotifier;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.Services.Impl;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using log4net.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeyImIn.WebApplication
{
	[ExcludeFromCodeCoverage]
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// ReSharper disable once UnusedMember.Global => This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add ASP.NET services
			string connectionString = _configuration.GetConnectionString("HeyImIn");
			services.AddDbContextPool<HeyImInDatabaseContext>(options => options.UseSqlServer(connectionString));
			services.AddResponseCompression();

			// Add global filters / attributes
			services
				.AddMvc(options => { options.Filters.Add(new LogActionAttribute()); })
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddJsonOptions(options =>
				{
					// Frontend expects property names to be camelCase and vice versa
#if DEBUG
					// In debug pretty print the JSON
					options.JsonSerializerOptions.WriteIndented = true;
#endif
					options.JsonSerializerOptions.IgnoreNullValues = true;
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});

			services.AddApiVersioning(
				options =>
				{
					options.ReportApiVersions = false;
					options.ErrorResponses = new ApiVersionErrorResponseProvider();
					options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
					options.AssumeDefaultVersionWhenUnspecified = false;
				});

			// Register all services as their matching interface
			// E.g. IMyService <-> MyService
			services.Scan(scan => scan
				.FromAssembliesOf(
					typeof(HomeController), // WebApplication
					typeof(IDatabaseContext), // Database
					typeof(INotificationService), // MailNotifier
					typeof(IAuthenticationService) // Authentication
				)
				.AddClasses()
				.AsMatchingInterface()
				.WithTransientLifetime());

			// Custom types with sensitive / global configuration
			var sendGridApiKey = _configuration.GetValue<string>("SENDGRID_API_KEY");

			// Other configuration values
			var configuration = new HeyImInConfiguration();
			_configuration.Bind("HeyImInConfiguration", configuration);

			// Register custom types
			services
				.AddSingleton(c => configuration)
				.AddSingleton<IHostedService, CronBackgroundService>()
				.AddScoped<IDatabaseContext, HeyImInDatabaseContext>() // Redirect interface to class
				.AddTransient<ISendGridClient>(c => new SendGridClient(sendGridApiKey))
				.AddTransient<ICronService, CronSendReminderAndSummaryService>()
				.AddTransient<ICronService, CronSendMissedChatMessagesService>()
				.AddTransient<GetDatabaseContext>(c => c.GetRequiredService<IDatabaseContext>);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime, IWebHostEnvironment env, ILoggerFactory loggerFactory, GetDatabaseContext contextFunc)
		{
			// Ensure swiss german date format is used
			var cultureInfo = new CultureInfo("de-CH");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
			}
			else
			{
				app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = c => Task.CompletedTask });
			}

			ConfigureLog4Net(env, loggerFactory);

			using (IDatabaseContext context = contextFunc())
			{
				context.Migrate(loggerFactory);
			}

			lifetime.ApplicationStarted.Register(LogStarted);
			lifetime.ApplicationStopping.Register(LogStopping);
			lifetime.ApplicationStopped.Register(LogStopped);

			app
				.UseResponseCompression()
				.UseRouting()
				.UseFileServer()
				.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
				});
		}

		private static void ConfigureLog4Net(IHostEnvironment env, ILoggerFactory loggerFactory)
		{
			// The mapping to the log4net.conf file is done through the AssemblyInfo.cs

			string logFileDirectory = env.IsDevelopment()
				? Path.Combine(env.ContentRootPath, "App_Data")
				: "/home/LogFiles/HeyImIn";

			if (!Directory.Exists(logFileDirectory))
			{
				// Make sure the directory exists
				Directory.CreateDirectory(logFileDirectory);
			}

			GlobalContext.Properties[LogHelpers.LogFileDirectoryKey] = logFileDirectory;

			// Hide (null) from logs => https://stackoverflow.com/a/22344774
			SystemInfo.NullText = string.Empty;

			loggerFactory.AddLog4Net();

			_logger = loggerFactory.CreateLogger<Startup>();
		}

		private static void LogStarted()
		{
			Version currentVersion = typeof(Startup).Assembly.GetName().Version ?? new Version();
			_logger.LogInformation("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
			_logger.LogInformation("{0}(): {1} Started at version {2}", nameof(LogStarted), StartEndPrefix, currentVersion);
			_logger.LogInformation("{0}(): {1}", nameof(LogStarted), StartEndPrefix);
		}

		private static void LogStopping()
		{
			if (_logger == null)
			{
				return;
			}

			_logger.LogInformation("{0}(): {1}", nameof(LogStopping), StartEndPrefix);
			_logger.LogInformation("{0}(): {1} Stopping...", nameof(LogStopping), StartEndPrefix);
			_logger.LogInformation("{0}(): {1}", nameof(LogStopping), StartEndPrefix);
		}

		private static void LogStopped()
		{
			if (_logger == null)
			{
				Trace.TraceError("No _logger configured -> Startup probably failed");
				return;
			}

			_logger.LogInformation("{0}(): {1}", nameof(LogStopped), StartEndPrefix);
			_logger.LogInformation("{0}(): {1} Stopped", nameof(LogStopped), StartEndPrefix);
			_logger.LogInformation("{0}(): {1}", nameof(LogStopped), StartEndPrefix);
		}

		private readonly IConfiguration _configuration;

		private const string StartEndPrefix = ">>>>>>>>";
		private static ILogger<Startup>? _logger;
	}
}
