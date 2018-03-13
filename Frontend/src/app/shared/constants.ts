export class Constants {
	// Thanks to https://stackoverflow.com/questions/11040707/c-sharp-regex-for-guid
	public static readonly guidRegex =
		'^[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}$';

	public static readonly userFullNameMaxLength = 40;
	public static readonly userEmailMaxLength = 40;
	public static readonly userPasswordHashMaxLength = 60;

	public static readonly titleMaxLength = 40;
	public static readonly meetingPlaceMaxLength = 40;
	public static readonly descriptionMaxLength = 120;
}
