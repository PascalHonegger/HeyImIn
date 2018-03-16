using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.WebApplication.Helpers;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Authenticates the user and calls <see cref="HttpActionExtensions.SetUserId" /> &
	///     <see cref="HttpActionExtensions.SetSessionToken" />
	/// </summary>
	public class AuthenticateUserAttribute : AuthorizeAttribute
	{
		private const string SessionTokenHttpHeaderKey = "SessionToken";

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			HttpRequestHeaders headers = actionContext.Request.Headers;
			if (!headers.Contains(SessionTokenHttpHeaderKey))
			{
				// No credentials provided
				return false;
			}

			string sessionToken = headers.GetValues(SessionTokenHttpHeaderKey).First();

			if (string.IsNullOrEmpty(sessionToken))
			{
				return false;
			}

			if (!Guid.TryParse(sessionToken, out Guid parsedSessionToken))
			{
				return false;
			}

			// Start the request in a new thread so the below .Wait call won't cause a deadlock
			Task<Session> getSessionTask = Task.Run(async () => await SessionService.GetAndExtendSessionAsync(parsedSessionToken));

			// Cannot use await as that's only supported in .NET core
			getSessionTask.Wait();

			Session session = getSessionTask.Result;

			if (session == null)
			{
				return false;
			}

			actionContext.Request.SetUserId(session.UserId);
			actionContext.Request.SetSessionToken(session.Token);

			return true;
		}

		// Set by DI
		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public ISessionService SessionService { get; set; }
	}
}