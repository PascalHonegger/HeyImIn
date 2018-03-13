using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class SetPasswordDto
	{
		[Required]
		public string CurrentPassword { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}
}