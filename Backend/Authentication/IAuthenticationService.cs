using System.Threading.Tasks;
using HeyImIn.Database.Models;

namespace HeyImIn.Authentication
{
	/// <summary>
	///     Encapsulates user authentication
	///     Automatically updates the users password if it <see cref="IPasswordService.NeedsRehash" />
	/// </summary>
	public interface IAuthenticationService
	{
		/// <summary>
		///     Tries to authenticate a user with the given credentials
		///     Automatically rehashes the password if required
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		Task<(bool authenticated, User foundUser)> AuthenticateAsync(string email, string password);
	}
}
