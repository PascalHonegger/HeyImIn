using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class Appointment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public virtual Event Event { get; set; }

		[Required]
		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		/// <summary>
		///     At what time the appointment will take place
		/// </summary>
		[Required]
		public DateTime StartTime { get; set; }

		public virtual ICollection<AppointmentParticipation> AppointmentParticipations { get; set; }
	}
}
