using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class EventInvitation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Token { get; set; }

		public virtual Event Event { get; set; }

		[Required]
		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		/// <summary>
		///     At what time the invitation was sent
		///     The invitation will expire after some period of time
		/// </summary>
		[Required]
		public DateTime Requested { get; set; }

		/// <summary>
		///     Wheter this invite was used
		/// </summary>
		public bool Used { get; set; }
	}
}
