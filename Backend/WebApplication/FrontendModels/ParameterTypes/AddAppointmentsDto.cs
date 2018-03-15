using System;
using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class AddAppointsmentsDto
	{
		public int EventId { get; set; }

		[Required]
		[MinLength(1)]
		public DateTime[] StartTimes { get; set; }
	}
}