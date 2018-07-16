using System;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentInformation
	{
		public AppointmentInformation(int appointmentId, DateTime startTime)
		{
			AppointmentId = appointmentId;
			StartTime = startTime;
		}

		public static AppointmentInformation FromAppointment(Appointment appointment)
		{
			var appointmentInformation = new AppointmentInformation(appointment.Id, appointment.StartTime);
			return appointmentInformation;
		}

		public int AppointmentId { get; }

		public DateTime StartTime { get; }
	}
}
