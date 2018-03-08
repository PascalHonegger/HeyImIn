namespace Authentication
{
	/// <summary>
	///     Encapsulates user authentication
	///     Automatically updates the users password if it <see cref="IPasswordService.NeedsRehash" />
	/// </summary>
	public interface IAuthenticationService
	{
	}
}
