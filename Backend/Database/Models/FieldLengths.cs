namespace HeyImIn.Database.Models
{
	/// <summary>
	///     Field lengths used by the database
	///     These values should be used in the backend & frontend validation
	///     CAUTION: Don't forget to update the data schema when these values get updated!
	/// </summary>
	public static class FieldLengths
	{
		public const int ChatMessageMaxLength = 250;
		public const int UserFullNameMaxLength = 40;
		public const int UserEmailMaxLength = 40;
		public const int UserPasswordHashMaxLength = 60;

		public const int TitleMaxLength = 40;
		public const int MeetingPlaceMaxLength = 40;
		public const int DescriptionMaxLength = 120;
		public const int RealisticMinimumHours = 1;
		public const int RealisticMaximumHours = 720; // 30 days
	}
}
