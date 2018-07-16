using System.Collections.Generic;
using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentDetails
	{
		public AppointmentDetails(AppointmentInformation appointmentInformation, List<AppointmentParticipationInformation> participations)
		{
			AppointmentInformation = appointmentInformation;
			Participations = participations;
		}

		public static AppointmentDetails FromAppointment(Appointment appointment, IEnumerable<int> allParticipantIds)
		{
			IEnumerable<AppointmentParticipationInformation> withAnswers = appointment.AppointmentParticipations.Select(p => new AppointmentParticipationInformation(p.ParticipantId, p.AppointmentParticipationAnswer));

			IEnumerable<int> otherParticipantIds = allParticipantIds.Except(appointment.AppointmentParticipations.Select(a => a.ParticipantId));
			IEnumerable<AppointmentParticipationInformation> noAnswers = otherParticipantIds.Select(id => new AppointmentParticipationInformation(id, null));

			List<AppointmentParticipationInformation> participations = withAnswers.Concat(noAnswers).ToList();
			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(appointment);

			return new AppointmentDetails(appointmentInformation, participations);
		}

		public AppointmentInformation AppointmentInformation { get; }

		public List<AppointmentParticipationInformation> Participations { get; }
	}
}
