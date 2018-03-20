using System.ComponentModel.DataAnnotations;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels
{
	public class GeneralEventInformation
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

		[Range(0, FieldLengths.RealisticMaximumHours)]
		public int ReminderTimeWindowInHours { get; set; }

		[Range(0, FieldLengths.RealisticMaximumHours)]
		public int SummaryTimeWindowInHours { get; set; }
	}
}