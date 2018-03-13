using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	public class SessionController : ApiController
	{
		public SessionController(IAuthenticationService authenticationService, ISessionService sessionService)
		{
			_authenticationService = authenticationService;
			_sessionService = sessionService;
		}

		/// <summary>
		///     Tries to start a new session for the provided credentials
		/// </summary>
		/// <returns>The created <see cref="FrontendSession" /> containing user information</returns>
		[HttpPost]
		[ResponseType(typeof(FrontendSession))]
		[AllowAnonymous]
		public async Task<IHttpActionResult> StartSession([FromBody] StartSessionDto startSessionDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (startSessionDto == null))
			{
				return BadRequest();
			}

			var (authenticated, foundUser) = await _authenticationService.AuthenticateAsync(startSessionDto.Email, startSessionDto.Password);

			if (!authenticated)
			{
				return Unauthorized();
			}

			Guid sessionToken = await _sessionService.CreateSessionAsync(foundUser.Id, true);

			_auditLog.InfoFormat("{0}(userId={1}): User logged in", nameof(StartSession), foundUser.Id);

			return Ok(new FrontendSession(sessionToken, foundUser.FullName, foundUser.Email));
		}

		/// <summary>
		///     Loads an already active session
		/// </summary>
		/// <param name="sessionToken">Unique session token</param>
		/// <returns>The found <see cref="FrontendSession" /></returns>
		[HttpGet]
		[ResponseType(typeof(FrontendSession))]
		[AllowAnonymous]
		public async Task<IHttpActionResult> GetSession(Guid sessionToken)
		{
			Session session = await _sessionService.GetAndExtendSessionAsync(sessionToken);

			if (session == null)
			{
				return Unauthorized();
			}

			return Ok(new FrontendSession(session.Token, session.User.FullName, session.User.Email));
		}

		/// <summary>
		///     Stops the active session => Log out and invalidate session
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		[AuthenticateUser]
		public async Task<IHttpActionResult> StopActiveSession()
		{
			Guid token = Request.GetSessionToken();

			await _sessionService.InvalidateSessionAsync(token);

			_auditLog.InfoFormat("{0}(): User logged out", nameof(StopActiveSession));

			return Ok();
		}

		private readonly IAuthenticationService _authenticationService;
		private readonly ISessionService _sessionService;
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}
