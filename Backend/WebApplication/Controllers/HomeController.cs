using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	public class HomeController : ControllerBase
	{
		private readonly IHostingEnvironment _environment;

		public HomeController(IHostingEnvironment environment)
		{
			_environment = environment;
		}

		/// <summary>
		///     Default / fallback route which redirects to index.html
		/// </summary>
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(404)]
		public PhysicalFileResult Index()
		{
			return PhysicalFile(Path.Combine(_environment.WebRootPath, "index.html"), "text/html");
		}
	}
}
