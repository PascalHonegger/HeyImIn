using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyImIn.Database.Models
{
	public class ChatMessage
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public virtual User Author { get; set; }

		[Required]
		[ForeignKey(nameof(Author))]
		public int AuthorId { get; set; }

		public virtual Event Event { get; set; }

		[Required]
		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		/// <summary>
		///     The time this chat message was sent / received by the server
		/// </summary>
		[Required]
		public DateTime SentDate { get; set; }

		/// <summary>
		///     The main message
		/// </summary>
		[Required]
		[MaxLength(FieldLengths.ChatMessageMaxLength)]
		public string Content { get; set; }
	}
}
