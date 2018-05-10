using System;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyImIn.WebApplication.Controllers
{
	[ApiController]
	[Route("api/Session")]
	public class SessionController : ControllerBase
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
		[HttpPost(nameof(StartSession))]
		[ProducesResponseType(typeof(FrontendSession), 200)]
		[AllowAnonymous]
		public async Task<IActionResult> StartSession(StartSessionDto startSessionDto)
		{
			(bool authenticated, User foundUser) = await _authenticationService.AuthenticateAsync(startSessionDto.Email, startSessionDto.Password);

			if (!authenticated)
			{
				return Unauthorized();
			}

			Guid sessionToken = await _sessionService.CreateSessionAsync(foundUser.Id, true);

			_auditLog.InfoFormat("{0}(userId={1}): User logged in", nameof(StartSession), foundUser.Id);

			return Ok(new FrontendSession(sessionToken, foundUser.Id, foundUser.FullName, foundUser.Email));
		}

		/// <summary>
		///     Loads an already active session
		/// </summary>
		/// <param name="sessionToken">Unique session token</param>
		/// <returns>The found <see cref="FrontendSession" /></returns>
		[HttpGet(nameof(GetSession))]
		[ProducesResponseType(typeof(FrontendSession), 200)]
		[AllowAnonymous]
		public async Task<IActionResult> GetSession(Guid sessionToken)
		{
			Session session = await _sessionService.GetAndExtendSessionAsync(sessionToken);

			if (session == null)
			{
				return Unauthorized();
			}

			return Ok(new FrontendSession(session.Token, session.UserId, session.User.FullName, session.User.Email));
		}

		/// <summary>
		///     Stops the active session => Log out and invalidate session
		/// </summary>
		[HttpPost(nameof(StopActiveSession))]
		[ProducesResponseType(typeof(void), 200)]
		[AuthenticateUser]
		public async Task<IActionResult> StopActiveSession()
		{
			Guid token = HttpContext.GetSessionToken();

			await _sessionService.InvalidateSessionAsync(token);

			_auditLog.InfoFormat("{0}(): User logged out", nameof(StopActiveSession));

			return Ok();
		}

		private readonly IAuthenticationService _authenticationService;
		private readonly ISessionService _sessionService;
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
