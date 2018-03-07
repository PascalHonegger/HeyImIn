using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class Event
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		///     The <see cref="User" /> which organizes this event
		///     Can manage <see cref="Appointments" />, general information and <see cref="EventParticipations" />
		/// </summary>
		public virtual User Organizer { get; set; }

		[Required]
		[ForeignKey(nameof(Organizer))]
		public int OrganizerId { get; set; }

		/// <summary>
		///     The place to meet at
		/// </summary>
		[Required]
		public string MeetingPlace { get; set; }

		/// <summary>
		///     A short description on what to expect from this event
		/// </summary>
		[Required]
		public string Description { get; set; }

		/// <summary>
		///     The amount of hours before an <see cref="Appointment" /> where the <see cref="EventParticipations" /> get a
		///     reminder email
		/// </summary>
		[Required]
		[Range(0, int.MaxValue)]
		public int ReminderTimeWindowInHours { get; set; }

		/// <summary>
		///     The amount of hours before an <see cref="Appointment" /> where the <see cref="EventParticipations" /> get a summary
		///     email
		///     Every change that happens after a summary mail was sent will trigger an updated summary to be sent
		/// </summary>
		[Required]
		[Range(0, int.MaxValue)]
		public int SummaryTimeWindowInHours { get; set; }

		public virtual ICollection<Appointment> Appointments { get; set; }

		public virtual ICollection<EventParticipation> EventParticipations { get; set; }
	}
}
