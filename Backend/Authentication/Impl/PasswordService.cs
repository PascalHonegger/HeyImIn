namespace HeyImIn.Authentication.Impl
{
	public class PasswordService : IPasswordService
	{
		/// <summary>
		///     Constructor for <see cref="IPasswordService" />
		/// </summary>
		/// <param name="workFactor">Defines how long the hashing for a password takes => Adjust as computing power increases</param>
		public PasswordService(int workFactor)
		{
			_workFactor = workFactor;
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
