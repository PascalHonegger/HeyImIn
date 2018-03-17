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

		public static AppointmentDetails FromAppointment(Appointment appointment, int currentUserId, List<User> allParticipants)
		{
			IEnumerable<AppointmentParticipationInformation> withAnswers = appointment.AppointmentParticipations.Select(p => new AppointmentParticipationInformation(p.Participant.FullName, p.Participant.Id, p.AppointmentParticipationAnswer));

			IEnumerable<User> otherParticipants = allParticipants.Except(appointment.AppointmentParticipations.Select(a => a.Participant));
			IEnumerable<AppointmentParticipationInformation> noAnswers = otherParticipants.Select(p => new AppointmentParticipationInformation(p.FullName, p.Id, null));

			List<AppointmentParticipationInformation> participations = withAnswers.Concat(noAnswers).ToList();
			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(appointment, currentUserId, allParticipants.Count);

			return new AppointmentDetails(appointmentInformation, participations);
		}

		public AppointmentInformation AppointmentInformation { get; }

		public List<AppointmentParticipationInformation> Participations { get; }
	}
}
