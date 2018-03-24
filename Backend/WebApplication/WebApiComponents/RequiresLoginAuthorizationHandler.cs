using System;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Authenticates the user and calls <see cref="HttpActionExtensions.SetUserId" /> &
	///     <see cref="HttpActionExtensions.SetSessionToken" />
	/// </summary>
	public class RequiresLoginAuthorizationHandler : AuthorizationHandler<RequiresLoginRequirement>
	{
		private readonly ISessionService _sessionService;
		private readonly IHttpContextAccessor _contextAccessor;

		public RequiresLoginAuthorizationHandler(ISessionService sessionService, IHttpContextAccessor contextAccessor)
		{
			_sessionService = sessionService;
			_contextAccessor = contextAccessor;
		}

		private const string SessionTokenHttpHeaderKey = "SessionToken";

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RequiresLoginRequirement requirement)
		{
			HttpContext httpContext = _contextAccessor.HttpContext;
			IHeaderDictionary headers = httpContext.Request.Headers;
			if (!headers.ContainsKey(SessionTokenHttpHeaderKey))
			{
				// No credentials provided
				context.Fail();
				return;
			}

			if (headers.TryGetValue(SessionTokenHttpHeaderKey, out StringValues sessionToken) || string.IsNullOrEmpty(sessionToken))
			{
				context.Fail();
				return;
			}

			if (!Guid.TryParse(sessionToken, out Guid parsedSessionToken))
			{
				context.Fail();
				return;
			}

			Session session = await _sessionService.GetAndExtendSessionAsync(parsedSessionToken);

			if (session == null)
			{
				context.Fail();
				return;
			}

			httpContext.SetUserId(session.UserId);
			httpContext.SetSessionToken(session.Token);

			context.Succeed(requirement);
		}
	}
}