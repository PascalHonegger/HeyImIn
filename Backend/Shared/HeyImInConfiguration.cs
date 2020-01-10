using System;
using System.Diagnostics.CodeAnalysis;

namespace HeyImIn.Shared
{
	/// <summary>
	///     The configuration specific to the behaviour of the HeyImIn software
	///     For descriptions of the values see the appsettings.json file, where settings can be explicitly specified
	///     CAUTION: The default values should be kept in sync with the default appsettings.json
	/// </summary>
	[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
	public class HeyImInConfiguration
	{
		public Uri FrontendBaseUrl { get; set; } = new Uri("https://hey-im-in.azurewebsites.net/#/", UriKind.Absolute);

		public string MailTimeZoneName { get; set; } = "W. Europe Standard Time";

		public string SenderEmailName { get; set; } = "Hey, I'm in";

		public string SenderEmailAddress { get; set; } = "no-reply@hey-im-in.ch";

		public int PasswordHashWorkFactor { get; set; } = 10;

		public int MaxAmountOfAppointmentsPerDetailPage { get; set; } = 5;

		public int BaseAmountOfChatMessagesPerDetailPage { get; set; } = 25;

		public HeyImInTimeSpans TimeSpans { get; } = new HeyImInTimeSpans();

		public class HeyImInTimeSpans
		{
			// Intervals
			public TimeSpan CronHandlerInterval { get; set; } = TimeSpan.FromMinutes(5);

			// TimeSpans
			public TimeSpan UpdateValidUntilTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

			public TimeSpan MinimumChatMessageNotificationTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

			// Timeouts
			public TimeSpan InactiveSessionTimeout { get; set; } = TimeSpan.FromDays(7);

			public TimeSpan UnusedSessionExpirationTimeout { get; set; } = TimeSpan.FromDays(2);

			public TimeSpan PasswordResetTimeout { get; set; } = TimeSpan.FromHours(2);

			public TimeSpan InviteTimeout { get; set; } = TimeSpan.FromDays(7);
		}
	}
}
