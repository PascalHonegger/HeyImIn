using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class AppointmentParticipation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public virtual Appointment Appointment { get; set; }

		[Required]
		[ForeignKey(nameof(Appointment))]
		public int AppointmentId { get; set; }

		public virtual User Participant { get; set; }

		[Required]
		[ForeignKey(nameof(Participant))]
		public int ParticipantId { get; set; }

		public AppointmentParticipationAnswer? AppointmentParticipationAnswer { get; set; }

		/// <summary>
		///     Whether a reminder has already been sent
		/// </summary>
		public bool SentReminder { get; set; }

		/// <summary>
		///     Whether a summary has already been sent
		///     If this is true and any changes occur, an updated summary is sent
		/// </summary>
		public bool SentSummary { get; set; }
	}
}
