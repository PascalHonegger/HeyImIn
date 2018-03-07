using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		///     The full name of the user, which is used to refer to him
		/// </summary>
		[Required]
		[MinLength(2)]
		[MaxLength(40)]
		public string FullName { get; set; }

		/// <summary>
		///     The email of the users, which is used for notifications and <see cref="PasswordReset" />
		/// </summary>
		[Required]
		[EmailAddress]
		[MaxLength(40)]
		public string Email { get; set; }

		/// <summary>
		///     The secure password hash
		/// </summary>
		[Required]
		[MaxLength(60)]
		public string PasswordHash { get; set; }

		public virtual ICollection<EventParticipation> EventParticipations { get; set; }
	}
}
