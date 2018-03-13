﻿namespace HeyImIn.Database.Models
{
	/// <summary>
	///     Field lenghts used by the database
	///     These values should be used in the backend & frontend validation
	/// </summary>
	public static class FieldLengths
	{
		public const int UserFullNameMaxLength = 40;
		public const int UserEmailMaxLength = 40;
		public const int UserPasswordHashMaxLength = 60;

		public const int TitleMaxLength = 40;
		public const int MeetingPlaceMaxLength = 40;
		public const int DescriptionMaxLength = 120;
	}
}