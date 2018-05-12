using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Authenticates the user and calls <see cref="HttpActionExtensions.SetUserId" /> &
	///     <see cref="HttpActionExtensions.SetSessionToken" />
	/// </summary>
	public class AuthenticateUserAttribute : Attribute, IAsyncAuthorizationFilter, IFilterFactory
	{
		public AuthenticateUserAttribute()
		{
		}

		public AuthenticateUserAttribute(ISessionService sessionService)
		{
			SessionService = sessionService;
		}


		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			if (context.HttpContext.Request.Headers.TryGetValue(SessionToken, out StringValues rawSessionToken))
			{
				string token = rawSessionToken.First();
				if (!string.IsNullOrEmpty(token) && Guid.TryParse(token, out Guid parsedToken))
				{
					Session session = await SessionService.GetAndExtendSessionAsync(parsedToken);

					if (session != null)
					{
						context.HttpContext.SetSessionToken(session.Token);
						context.HttpContext.SetUserId(session.UserId);
						return;
					}
				}
			}

			context.Result = new UnauthorizedResult();
		}

		public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
		{
			var sessionService = serviceProvider.GetService<ISessionService>();
			return new AuthenticateUserAttribute(sessionService);
		}

		public bool IsReusable { get; } = false;

		private ISessionService SessionService { get; }

		private const string SessionToken = "SessionToken";
	}
}
