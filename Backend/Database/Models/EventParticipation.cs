using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class EventParticipation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public virtual Event Event { get; set; }

		[Required]
		[ForeignKey(nameof(Event))]
		[Index("UniqueEventParticipation", IsUnique = true, Order = 1)]
		public int EventId { get; set; }

		public virtual User Participant { get; set; }

		[Required]
		[ForeignKey(nameof(Participant))]
		[Index("UniqueEventParticipation", IsUnique = true, Order = 2)]
		public int ParticipantId { get; set; }

		/// <summary>
		///     Wheter or not the users wants to receive an email to remind him of his participation
		/// </summary>
		public bool SendReminderEmail { get; set; } = true;

		/// <summary>
		///     Wheter or not the users wants to receive an email with the final event information
		/// </summary>
		public bool SendSummaryEmail { get; set; } = true;

		/// <summary>
		///     Wheter or not the users wants to receive an email to inform him about last-minute changes (changes made after the
		///     summary was sent)
		/// </summary>
		public bool SendLastMinuteChangesEmail { get; set; } = true;
	}
}
