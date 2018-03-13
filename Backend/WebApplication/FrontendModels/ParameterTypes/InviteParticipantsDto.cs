using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class InviteParticipantsDto
	{
		public int EventId { get; set; }

		[Required]
		[MinLength(1)]
		public List<string> EmailAddresses { get; set; }
	}
}