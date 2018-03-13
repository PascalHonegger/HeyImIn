using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class AddAppointsmentsDto
	{
		public int EventId { get; set; }

		[Required]
		[MinLength(1)]
		public List<DateTime> StartTimes { get; set; }
	}
}