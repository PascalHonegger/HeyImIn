namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class UpdatedEventInfoDto : GeneralEventInformation
	{
		public int EventId { get; set; }

		// In the future make the organizer changeable? => public int OrganizerId { get; }
	}
}
