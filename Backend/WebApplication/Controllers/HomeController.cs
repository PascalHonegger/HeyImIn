using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	public class HomeController : Controller
	{
		/// <summary>
		///     Default / fallback route which redirects to index.html
		/// </summary>
		[HttpGet]
		[Route("")]
		public HttpResponseMessage Index()
		{
			// Taken from MediaGateway project
			var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
			response.Headers.Location = new Uri("client/index.html", UriKind.Relative);
			response.Headers.CacheControl = CacheControlHeaderValue.Parse("no-cache, no-store, must-revalidate");
			return response;
		}
	}
}
