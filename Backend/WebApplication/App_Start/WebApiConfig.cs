using System;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;
using Autofac;
using Autofac.Features.ResolveAnything;
using Autofac.Integration.WebApi;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.External.DependencyInjection;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.WebApiComponents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SendGrid;

namespace HeyImIn.WebApplication
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			BuildContainer(config);
			ApplyFormatters(config);
			ConfigureWebApi(config);
		}

		private static void ConfigureWebApi(HttpConfiguration config)
		{
#if DEBUG
			// For debugging allow all sources
			var enableForAllAttribute = new System.Web.Http.Cors.EnableCorsAttribute("*", "*", "*");
			config.EnableCors(enableForAllAttribute);
#endif
			// Filters / Attributes which apply for all methods
			config.Filters.Add(new LogActionAttribute());

			// Web API routes
			config.MapHttpAttributeRoutes();

			// Everything not below api/* will be redirected to the angular index.html file
			config.Routes.MapHttpRoute(
				"DefaultApi",
				"api/{controller}/{action}",
				new { controller = "Home", action = "Index" }
			);
		}

		private static void ApplyFormatters(HttpConfiguration config)
		{
			config.Formatters.Clear();
			config.Formatters.Add(new JsonMediaTypeFormatter());
			// Frontend expects property names to be camelCase and vice versa
			config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
#if DEBUG
			// In debug pretty print the JSON
			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
#endif
			config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			// Convert all times to UTC
			config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
		}

		private static void BuildContainer(HttpConfiguration config)
		{
			var builder = new ContainerBuilder();

			// Add future assemblies here to ensure the types can be resolved
			AssemblyScanner.Scan(builder,
				typeof(HomeController).Assembly,			// WebApplication
				typeof(IDatabaseContext).Assembly,			// Database
				typeof(INotificationService).Assembly,		// MailNotifier
				typeof(IAuthenticationService).Assembly		// Authentication
			);

			// Custom types with sensitive / global configuration
			string connectionString = ConfigurationManager.ConnectionStrings["HeyImIn"].ConnectionString;
			string sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

			// Register custom types
			builder.Register(c => new HeyImInDatabaseContext(connectionString)).As<IDatabaseContext>();
			builder.Register(c => new SendGridClient(sendGridApiKey)).As<ISendGridClient>();

			// Register WebApi controllers & attributes
			builder.RegisterApiControllers(typeof(HomeController).Assembly).InstancePerRequest();
			builder.RegisterWebApiFilterProvider(config);

			// Register other types
			builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

			// Set the dependency resolver to be Autofac
			IContainer container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}
