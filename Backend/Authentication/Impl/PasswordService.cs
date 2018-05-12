using HeyImIn.Shared;

namespace HeyImIn.Authentication.Impl
{
	public class PasswordService : IPasswordService
	{
		public PasswordService(HeyImInConfiguration configuration)
		{
			_workFactor = configuration.PasswordHashWorkFactor;
		}

		public string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
		}

		public bool VerifyPassword(string password, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(password, hash);
		}

		public bool NeedsRehash(string hash)
		{
			return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, _workFactor);
		}

		private readonly int _workFactor;
	}
}
