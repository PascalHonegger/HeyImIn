using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class SetUserDataDto
	{
		[Required]
		[MaxLength(FieldLengths.UserFullNameMaxLength)]
		public string NewFullName { get; set; }

		[Required]
		[EmailAddress]
		[MaxLength(FieldLengths.UserEmailMaxLength)]
		public string NewEmail { get; set; }
	}
}
