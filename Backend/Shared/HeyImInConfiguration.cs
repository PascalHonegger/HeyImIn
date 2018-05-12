using System;

namespace HeyImIn.Shared
{
	/// <summary>
	///     The configuration specific to the behaviour of the HeyImIn software
	///     For descriptions of the values see the appsettings.json file, where settings can be explicitly specified
	///     CAUTION: The default values should be keept in sync with the default appsettings.json
	/// </summary>
	public class HeyImInConfiguration
	{
		public string FrontendBaseUrl { get; set; } = "https://hey-im-in.azurewebsites.net/#/";

		public string MailTimeZoneName { get; set; } = "W. Europe Standard Time";

		public int PasswordHashWorkFactor { get; set; } = 10;

		public HeyImInTimeouts Timeouts { get; } = new HeyImInTimeouts();

		public class HeyImInTimeouts
		{
			public TimeSpan InactiveSessionTimeout { get; set; } = TimeSpan.FromHours(2);

			public TimeSpan UnusedSessionExpirationTimeout { get; set; } = TimeSpan.FromDays(2);

			public TimeSpan PasswordResetTimeout { get; set; } = TimeSpan.FromHours(2);

			public TimeSpan InviteTimeout { get; set; } = TimeSpan.FromDays(7);
		}
	}
}
