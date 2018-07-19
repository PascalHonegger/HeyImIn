using System;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class GetChatMessagesDto
	{
		/// <summary>
		///     The id of the event
		/// </summary>
		public int EventId { get; set; }

		/// <summary>
		///     Null or the earliest SentDate from previously loaded data
		/// </summary>
		public DateTime? EarliestLoadedMessageSentDate { get; set; }
	}
}
