using System;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.Authentication.Impl
{
	public class SessionService : ISessionService
	{
		public SessionService(HeyImInConfiguration configuration, GetDatabaseContext getDatabaseContext, ILogger<SessionService> logger)
		{
			_inactiveSessionTimeout = configuration.Timeouts.InactiveSessionTimeout;
			_unusedSessionExpirationTimeout = configuration.Timeouts.UnusedSessionExpirationTimeout;
			_getDatabaseContext = getDatabaseContext;
			_logger = logger;
		}

		public async Task<Session> GetAndExtendSessionAsync(Guid token)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Session session = await context.Sessions.Include(s => s.User).FirstOrDefaultAsync(s => s.Token == token);

				if ((session == null) || !IsValidSession(session))
				{
					return null;
				}

				ActivateOrExtendSession(session);

				await context.SaveChangesAsync();

				return session;
			}
		}

		public async Task<Guid> CreateSessionAsync(int userId, bool active)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				var session = new Session
				{
					UserId = userId,
					Created = DateTime.UtcNow
				};

				if (active)
				{
					ActivateOrExtendSession(session);
				}

				context.Sessions.Add(session);

				await context.SaveChangesAsync();

				_logger.LogDebug("{0}(userId={1}, active={2}): Added new session", nameof(CreateSessionAsync), userId, active);

				return session.Token;
			}
		}

		public async Task InvalidateSessionAsync(Guid token)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				Session session = await context.Sessions.FindAsync(token);

				if ((session == null) || !IsValidSession(session))
				{
					return;
				}

				session.ValidUntil = DateTime.UtcNow;

				await context.SaveChangesAsync();

				_logger.LogDebug("{0}(token={1}): Invalidated session", nameof(InvalidateSessionAsync), token);
			}
		}

		private void ActivateOrExtendSession(Session session)
		{
			// Set new valid until date as the session has been used
			session.ValidUntil = DateTime.UtcNow + _inactiveSessionTimeout;
		}

		private bool IsValidSession(Session session)
		{
			return session.ValidUntil == null
				? DateTime.UtcNow - session.Created <= _unusedSessionExpirationTimeout
				: session.ValidUntil >= DateTime.UtcNow;
		}


		private readonly GetDatabaseContext _getDatabaseContext;

		/// <summary>
		///     A session gets invalidated after this time period passed without any access to the session
		/// </summary>
		private readonly TimeSpan _inactiveSessionTimeout;

		/// <summary>
		///     If a session is not accessed after this time, it expires and turns invalid
		/// </summary>
		private readonly TimeSpan _unusedSessionExpirationTimeout;

		private readonly ILogger<SessionService> _logger;
	}
}
