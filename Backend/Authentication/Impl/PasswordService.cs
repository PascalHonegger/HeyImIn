namespace HeyImIn.Authentication.Impl
{
	public class PasswordService : IPasswordService
	{
		/// <summary>
		/// Defines how long the hashing for a password takes => Adjust as computing power increases
		/// </summary>
		private const int WorkFactor = 10;

		public string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
		}

		public bool VerifyPassword(string password, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(password, hash);
		}

		public bool NeedsRehash(string hash)
		{
			return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, WorkFactor);
		}
	}
}