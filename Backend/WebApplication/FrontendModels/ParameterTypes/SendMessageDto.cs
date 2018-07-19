using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class SendMessageDto
	{
		public int EventId { get; set; }

		[Required]
		[MinLength(1)]
		[MaxLength(FieldLengths.ChatMessageMaxLength)]
		public string Content { get; set; }
	}
}
