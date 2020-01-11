using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Shared;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Controllers
{
	[ApiController]
	[ApiVersion(ApiVersions.Version2_0)]
	[Route("api/Session")]
	public class SessionController : ControllerBase
	{
		public SessionController(IAuthenticationService authenticationService, ISessionService sessionService, GetDatabaseContext getDatabaseContext, ILoggerFactory loggerFactory)
		{
			_authenticationService = authenticationService;
			_sessionService = sessionService;
			_getDatabaseContext = getDatabaseContext;
			_auditLogger = loggerFactory.CreateAuditLogger();
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
			(bool authenticated, User? foundUser) = await _authenticationService.AuthenticateAsync(startSessionDto.Email, startSessionDto.Password);

			if (!authenticated)
			{
				return Unauthorized();
			}

			Guid sessionToken = await _sessionService.CreateSessionAsync(foundUser!.Id, true);

			_auditLogger.LogInformation("{0}(userId={1}): User logged in", nameof(StartSession), foundUser.Id);

			return Ok(new FrontendSession(sessionToken, foundUser.Id, foundUser.FullName, foundUser.Email));
		}

		/// <summary>
		///     Loads an already active session
		/// </summary>
		/// <param name="sessionToken">Unique session token</param>
		/// <returns>The found <see cref="FrontendSession" /></returns>
		[HttpGet(nameof(GetSession))]
		[ProducesResponseType(typeof(FrontendSession), 200)]
		[ProducesResponseType(typeof(void), 401)]
		[AllowAnonymous]
		public async Task<IActionResult> GetSession(Guid sessionToken)
		{
			Session? session = await _sessionService.GetAndExtendSessionAsync(sessionToken);

			if (session == null)
			{
				return Unauthorized();
			}

			IDatabaseContext context = _getDatabaseContext();
			User? user = await context.Users.FindAsync(session.UserId);

			if (user == null)
			{
				return Unauthorized();
			}

			return Ok(new FrontendSession(session.Token, session.UserId, user.FullName, user.Email));
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

			_auditLogger.LogInformation("{0}(): User logged out", nameof(StopActiveSession));

			return Ok();
		}

		private readonly IAuthenticationService _authenticationService;
		private readonly ISessionService _sessionService;
		private readonly GetDatabaseContext _getDatabaseContext;
		private readonly ILogger _auditLogger;
	}
}
