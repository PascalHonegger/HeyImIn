﻿using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class RemoveFromEventDto
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
		[Required]
		public int EventId { get; set; }

		[Required]
		public int UserId { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
	}
}
