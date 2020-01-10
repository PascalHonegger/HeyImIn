// ReSharper disable InconsistentNaming

using System;

namespace HeyImIn.WebApplication.Helpers
{
	public static class ApiVersions
	{
		public const string Version2_0 = "2.0";

		[Obsolete("Superseded by " + nameof(Version2_0))]
		public const string Version1_1 = "1.1";
	}
}
