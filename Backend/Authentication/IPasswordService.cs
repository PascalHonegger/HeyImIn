namespace HeyImIn.Authentication
{
	/// <summary>
	///     Encapsulates
	/// </summary>
	public interface IPasswordService
	{
		/// <summary>
		///     Hashes the provided
		///     <param name="password"></param>
		/// </summary>
		/// <param name="password">The password to hash</param>
		/// <returns>Hash of the password</returns>
		string HashPassword(string password);

		/// <summary>
		///     Verifies the provided password
		/// </summary>
		/// <param name="password">Password to validate</param>
		/// <param name="hash">Hash to test against</param>
		/// <returns>True if the password matches the hash</returns>
		bool VerifyPassword(string password, string hash);

		/// <summary>
		///     Tests wheter a password needs to be rehashed in order to stay secure (E.g. hashing algorithm changed)
		/// </summary>
		/// <param name="hash">Hash to test</param>
		/// <returns>True if the password needs to be rehashed</returns>
		bool NeedsRehash(string hash);
	}
}
