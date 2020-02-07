using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class SetUserDataDto
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
		[Required]
		[MaxLength(FieldLengths.UserFullNameMaxLength)]
		public string NewFullName { get; set; }

		[Required]
		[EmailAddress]
		[MaxLength(FieldLengths.UserEmailMaxLength)]
		public string NewEmail { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
	}
}
