using System;
using System.Globalization;

namespace HeyImIn.WebApplication.Helpers
{
	public static class DateTimeExtensions
	{
		public static string ToHeyImInString(this DateTime dateTime)
		{
			return dateTime.ToString("F", new CultureInfo("de-ch"));
		}
	}
}
