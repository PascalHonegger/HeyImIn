using System;
using System.Linq;
using HeyImIn.Database.Models;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class AppointmentInformation
	{
		public AppointmentInformation(int id, DateTime startTime, AppointmentParticipationAnswer? currentResponse, int acceptedParticipants, int declinedParticipants, int notAnsweredParticipants)
		{
			Id = id;
			StartTime = startTime;
			CurrentResponse = currentResponse;
			AcceptedParticipants = acceptedParticipants;
			DeclinedParticipants = declinedParticipants;
			NotAnsweredParticipants = notAnsweredParticipants;
		}

		public static AppointmentInformation FromAppointment(Appointment appointment, User currentUser, int totalCount)
		{
			int acceptedCount = appointment.AppointmentParticipations.Count(a => a.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted);
			int declinedCount = appointment.AppointmentParticipations.Count(a => a.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Declined);
			int noAnswerCount = totalCount - (acceptedCount + declinedCount);

			AppointmentParticipationAnswer? currentUserAnswer = appointment.AppointmentParticipations.FirstOrDefault(a => a.Participant == currentUser)?.AppointmentParticipationAnswer;

			var appointmentInformation = new AppointmentInformation(appointment.Id, appointment.StartTime, currentUserAnswer, acceptedCount, declinedCount, noAnswerCount);
			return appointmentInformation;
		}

		public int Id { get; }

		public DateTime StartTime { get; }

		public AppointmentParticipationAnswer? CurrentResponse { get; }

		public int AcceptedParticipants { get; }

		public int DeclinedParticipants { get; }

		public int NotAnsweredParticipants { get; }
	}
}
