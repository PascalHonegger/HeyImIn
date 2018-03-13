using System.Collections.Generic;
using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentDetails
	{
		public AppointmentDetails(AppointmentInformation appointmentInformation, List<AppointmentParticipation> participations)
		{
			AppointmentInformation = appointmentInformation;
			Participations = participations;
		}

		public static AppointmentDetails FromAppointment(Appointment appointment, User currentUser, List<User> allParticipants)
		{
			IEnumerable<AppointmentParticipation> withAnswers = appointment.AppointmentParticipations.Select(p => new AppointmentParticipation(p.Participant.FullName, p.Participant.Id, p.AppointmentParticipationAnswer));

			IEnumerable<User> otherParticipants = allParticipants.Except(appointment.AppointmentParticipations.Select(a => a.Participant));
			IEnumerable<AppointmentParticipation> noAnswers = otherParticipants.Select(p => new AppointmentParticipation(p.FullName, p.Id, null));

			List<AppointmentParticipation> participations = withAnswers.Concat(noAnswers).ToList();
			AppointmentInformation appointmentInformation = AppointmentInformation.FromAppointment(appointment, currentUser, allParticipants.Count);

			return new AppointmentDetails(appointmentInformation, participations);
		}

		public AppointmentInformation AppointmentInformation { get; }

		public List<AppointmentParticipation> Participations { get; }
	}
}
