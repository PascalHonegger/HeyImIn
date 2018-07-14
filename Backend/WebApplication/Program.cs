using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HeyImIn.WebApplication
{
	[ExcludeFromCodeCoverage]
	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		private static IWebHost BuildWebHost(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseKestrel()
				.UseIISIntegration()
				.UseAzureAppServices()
				.UseStartup<Startup>()
				.Build();
		}
	}
}
