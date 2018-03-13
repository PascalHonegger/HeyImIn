using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class RequestPasswordResetDto
	{
		[Required]
		[EmailAddress]
		[MaxLength(FieldLengths.UserEmailMaxLength)]
		public string Email { get; set; }
	}
}