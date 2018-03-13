using System;
using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class ResetPasswordDto
	{
		[Required]
		public Guid PasswordResetToken { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}
}