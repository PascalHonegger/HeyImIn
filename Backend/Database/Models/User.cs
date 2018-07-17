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
		[MaxLength(FieldLengths.UserFullNameMaxLength)]
		public string FullName { get; set; }

		/// <summary>
		///     The email of the users, which is used for notifications and <see cref="PasswordReset" />
		/// </summary>
		[Required]
		[EmailAddress]
		[MaxLength(FieldLengths.UserEmailMaxLength)]
		public string Email { get; set; }

		/// <summary>
		///     The secure password hash
		/// </summary>
		[Required]
		[MaxLength(FieldLengths.UserPasswordHashMaxLength)]
		public string PasswordHash { get; set; }

		public virtual ICollection<Session> Sessions { get; set; }

		public virtual ICollection<ChatMessage> ChatMessages { get; set; }

		public virtual ICollection<PasswordReset> PasswordResets { get; set; }

		public virtual ICollection<Event> OrganizedEvents { get; set; }

		public virtual ICollection<EventParticipation> EventParticipations { get; set; }

		public virtual ICollection<AppointmentParticipation> AppointmentParticipations { get; set; }
	}
}
