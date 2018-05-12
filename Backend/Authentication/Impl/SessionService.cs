using System;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Shared;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.Authentication.Impl
{
	public class SessionService : ISessionService
	{
		public SessionService(HeyImInConfiguration configuration, GetDatabaseContext getDatabaseContext)
		{
			_inactiveSessionTimeout = configuration.Timeouts.InactiveSessionTimeout;
			_getDatabaseContext = getDatabaseContext;
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

				_log.DebugFormat("{0}(userId={1}, active={2}): Added new session", nameof(CreateSessionAsync), userId, active);

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

				_log.DebugFormat("{0}(token={1}): Invalidated session", nameof(InvalidateSessionAsync), token);
			}
		}

		private void ActivateOrExtendSession(Session session)
		{
			// Set new valid until date as the session has been used
			session.ValidUntil = DateTime.UtcNow + _inactiveSessionTimeout;
		}

		private static bool IsValidSession(Session session)
		{
			return session.ValidUntil == null 
				? DateTime.UtcNow - session.Created <= TimeSpan.FromDays(2)
				: session.ValidUntil >= DateTime.UtcNow;
		}


		private readonly GetDatabaseContext _getDatabaseContext;

		/// <summary>
		///     A session gets invalidated after this time period passed without any access to the session
		/// </summary>
		private readonly TimeSpan _inactiveSessionTimeout;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
