export class Constants {
	// Thanks to https://stackoverflow.com/questions/11040707/c-sharp-regex-for-guid
	public static readonly guidRegex =
		'^[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}$';


	// The following constants are based on the Backend FieldLenghts.cs
	public static readonly chatMessageMaxLength = 250;
	public static readonly userFullNameMaxLength = 40;
	public static readonly userEmailMaxLength = 40;
	public static readonly userPasswordHashMaxLength = 60;

	public static readonly titleMaxLength = 40;
	public static readonly meetingPlaceMaxLength = 40;
	public static readonly descriptionMaxLength = 120;
	public static readonly realisticMinimumHours = 1;
	public static readonly realisticMaximumHours = 720;
}
