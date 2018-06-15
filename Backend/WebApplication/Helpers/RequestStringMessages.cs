namespace HeyImIn.WebApplication.Helpers
{
	/// <summary>
	///     Constant strings which are returned as error messages to the Frontend
	/// </summary>
	public static class RequestStringMessages
	{
		public const string EventNotFound = "Der angegebene Event wurde nicht gefunden";
		public const string UserNotFound = "Der angegebene Benutzer wurde nicht gefunden";
		public const string AppointmentNotFound = "Der angegebene Termin wurde nicht gefunden";

		public const string UserAlreadyPartOfEvent = "Der angegebene Benutzer nimmt bereits dem Event teil";
		public const string UserNotPartOfEvent = "Der angegebene Benutzer nimmt nicht an dem Event teil";

		public const string InvitationRequired = "Der angegebene Event ist privat und benötigt eine Einladung";
		public const string OrganizorRequired = "Sie müssen Organisator dieses Event sein, um diese Aktion durchzuführen";

		public const string AppointmentsHaveToStartInTheFuture = "Termine müssen in der Zukunft liegen, überprüfen Sie ihre Eingabe";

		public const string CurrentPasswordWrong = "Das angegebene jetzige Passwort ist nicht korrekt";
		public const string NoProfileWithEmailFound = "Für diese E-Mail-Adresse ist kein Konto hinterlegt";
		public const string EmailAlreadyInUse = "Für diese E-Mail-Adresse ist bereits ein Konto hinterlegt";

		public const string ResetCodeInvalid = "Der angegebene Code ist ungültig";
		public const string ResetCodeAlreadyUsedOrExpired = "Der angegebene Code ist abgelaufen oder wurde bereits verwendet";

		public const string InvitationInvalid = "Die Einladung ist ungültig";
		public const string InvitationAlreadyUsed = "Die Einladung wurde bereits verwendet";
	}
}
