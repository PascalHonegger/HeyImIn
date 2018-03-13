using System;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class ResetPasswordDto
	{
		public Guid PasswordResetToken { get; set; }
		public string NewPassword { get; set; }
	}
}