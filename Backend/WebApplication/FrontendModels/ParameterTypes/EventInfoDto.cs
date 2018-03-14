using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class EventInfoDto
	{
		[Required]
		[MaxLength(FieldLengths.TitleMaxLength)]
		public string Title { get; set; }

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