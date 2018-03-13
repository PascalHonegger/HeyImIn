using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class RegisterDto
	{
		public string FullName { get; set; }

		[EmailAddress]
		public string Email { get; set; }

		public string Password { get; set; }
	}
}