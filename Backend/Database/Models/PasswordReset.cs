using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class PasswordReset
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Token { get; set; }

		public virtual User User { get; set; }

		[Required]
		[ForeignKey(nameof(User))]
		public int UserId { get; set; }

		/// <summary>
		///     At what time the password reset was requested
		///     If not used for a period of time it is no longer valid
		/// </summary>
		[Required]
		public DateTime Requested { get; set; }

		/// <summary>
		///     Wheter this password reset was used
		/// </summary>
		public bool Used { get; set; }
	}
}
