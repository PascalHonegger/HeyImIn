using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class Session
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Token { get; set; }

		public virtual User User { get; set; }

		[Required]
		[ForeignKey(nameof(User))]
		public int UserId { get; set; }

		/// <summary>
		///     At what point the (active) session will be invalidated
		/// </summary>
		public DateTime? ValidUntil { get; set; }

		/// <summary>
		///     At what time the session was created
		///     If a session isn't activated within a certain period of time (<see cref="ValidUntil" /> == null) it will expire
		/// </summary>
		[Required]
		public DateTime Created { get; set; }
	}
}
