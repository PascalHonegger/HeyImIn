using System;
using System.Threading.Tasks;
using HeyImIn.Database.Models;

namespace HeyImIn.Authentication
{
	/// <summary>
	///     Encapsulates access to sessions
	/// </summary>
	public interface ISessionService
	{
		/// <summary>
		///     Tries to load a session. If one is found also extends / activates it.
		///     Does not load related data
		/// </summary>
		/// <param name="token">
		///     <see cref="Session.Token" />
		/// </param>
		/// <returns>Found session or null</returns>
		Task<Session> GetAndExtendSessionAsync(Guid token);

		/// <summary>
		///     Creates a new session for a user
		/// </summary>
		/// <param name="userId">The user the session belongs to</param>
		/// <param name="automaticallyActivate">Whether the session should be activated automatically</param>
		/// <returns>Token of the created session</returns>
		Task<Guid> CreateSessionAsync(int userId, bool automaticallyActivate);

		/// <summary>
		///     Invalidates a session if it is valid, else does nothing
		/// </summary>
		/// <param name="token">
		///     <see cref="Session.Token" />
		/// </param>
		Task InvalidateSessionAsync(Guid token);
	}
}
