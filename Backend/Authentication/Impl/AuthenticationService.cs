using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.Authentication.Impl
{
	public class AuthenticationService : IAuthenticationService
	{
		public AuthenticationService(IPasswordService passwordService, GetDatabaseContext getDatabaseContext, ILogger<AuthenticationService> logger)
		{
			_passwordService = passwordService;
			_getDatabaseContext = getDatabaseContext;
			_logger = logger;
		}

		public async Task<(bool authenticated, User foundUser)> AuthenticateAsync(string email, string password)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User foundUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
				if (foundUser == null)
				{
					_logger.LogInformation("{0}(): Failed authentication for not existing email {1}", nameof(AuthenticateAsync), email);

					// Wrong email
					return (false, null);
				}

				if (!_passwordService.VerifyPassword(password, foundUser.PasswordHash))
				{
					_logger.LogInformation("{0}(): Failed authentication for user {1} because of wrong password", nameof(AuthenticateAsync), foundUser.Id);

					// Wrong password
					return (false, foundUser);
				}

				if (_passwordService.NeedsRehash(foundUser.PasswordHash))
				{
					_logger.LogInformation("{0}(): Updating password hash for user {1} to match work factor", nameof(AuthenticateAsync), foundUser.Id);

					// Update password hash to keep it secure in case the hash factor got updated
					foundUser.PasswordHash = _passwordService.HashPassword(password);
					await context.SaveChangesAsync();
				}

				return (true, foundUser);
			}
		}

		private readonly IPasswordService _passwordService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private readonly ILogger<AuthenticationService> _logger;
	}
}
