using System;
using HeyImIn.WebApplication.Helpers;

namespace HeyImIn.WebApplication.WebApiComponents
{
	/// <summary>
	///     Authenticates the user and calls <see cref="HttpActionExtensions.SetUserId" /> &
	///     <see cref="HttpActionExtensions.SetSessionToken" />
	/// </summary>
	public class AuthenticateUserAttribute : Attribute // TODO : AuthorizeAttribute
	{
		public AuthenticateUserAttribute() // TODO : base("RequiresLogin")
		{
		}
	}
}