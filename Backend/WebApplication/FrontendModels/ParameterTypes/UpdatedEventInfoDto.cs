using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class UpdatedEventInfoDto
	{
		public int EventId { get; set; }

		// In the future make the organizer changeable? => public int OrganizerId { get; }

		[Required]
		[MaxLength(FieldLengths.MeetingPlaceMaxLength)]
		public string MeetingPlace { get; set; }

		[Required]
		[MaxLength(FieldLengths.DescriptionMaxLength)]
		public string Description { get; set; }

		public bool IsPrivate { get; set; }

		[Range(0, int.MaxValue)]
		public int ReminderTimeWindowInHours { get; set; }

		[Range(0, int.MaxValue)]
		public int SummaryTimeWindowInHours { get; set; }
	}
}