using System;

namespace HeyImIn.WebApplication.FrontendModels
{
	public class ResetPasswordDto
	{
		public Guid PasswordResetToken { get; set; }
		public string NewPassword { get; set; }
	}
}