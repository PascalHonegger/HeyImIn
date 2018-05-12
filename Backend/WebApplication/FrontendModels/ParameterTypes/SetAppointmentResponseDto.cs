using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ParameterTypes
{
	public class SetAppointmentResponseDto
	{
		public int AppointmentId { get; set; }

		public int UserId { get; set; }

		public AppointmentParticipationAnswer? Response { get; set; }
	}
}
