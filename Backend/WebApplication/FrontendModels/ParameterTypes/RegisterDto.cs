using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class RegisterDto
	{
		[Required]
		[MaxLength(FieldLengths.UserFullNameMaxLength)]
		public string FullName { get; set; }

		[Required]
		[EmailAddress]
		[MaxLength(FieldLengths.UserEmailMaxLength)]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
