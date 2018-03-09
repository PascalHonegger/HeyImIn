using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels
{
	public class RegisterDto
	{
		public string FullName { get; set; }

		[EmailAddress]
		public string Email { get; set; }

		public string Password { get; set; }
	}
}