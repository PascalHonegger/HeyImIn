using System.Collections.Generic;
using System.Linq;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier.Models;

namespace HeyImIn.WebApplication.Services.Impl
{
	public class DeleteService : IDeleteService
	{
		private static EventNotificationInformation ExtractNotificationInformation(Event @event)
		{
			return new EventNotificationInformation(@event.Id, @event.Title, @event.EventParticipations
				.Select(p => ExtractNotificationInformation(p.Participant))
				.ToList());
		}

		private static UserNotificationInformation ExtractNotificationInformation(User user)
		{
			return new UserNotificationInformation(user.Id, user.FullName, user.Email);
		}

		private static AppointmentParticipationNotificationInformation ExtractNotificationInformation(AppointmentParticipation participation)
		{
			return new AppointmentParticipationNotificationInformation(
				ExtractNotificationInformation(participation.Participant),
				participation.AppointmentParticipationAnswer,
				participation.SentSummary,
				participation.SentReminder);
		}

		public List<EventNotificationInformation> DeleteUserLocally(IDatabaseContext context, User user)
		{
			// Backup data relevant for sending notifications
			List<Event> organizedEvents = user.OrganizedEvents.ToList();

			// Events the user organized
			var deletedEventNotificationInfos = new List<EventNotificationInformation>();

			foreach (Event organizedEvent in organizedEvents)
			{
				EventNotificationInformation deletedInfo = DeleteEventLocally(context, organizedEvent);
				deletedEventNotificationInfos.Add(deletedInfo);
			}

			// Token based relations
			context.Sessions.RemoveRange(user.Sessions);
			context.PasswordResets.RemoveRange(user.PasswordResets);

			// Event participations the user is part of
			context.EventParticipations.RemoveRange(user.EventParticipations);

			// Appointment participations the user is part of
			List<AppointmentParticipation> userAppointmentParticipations = user.AppointmentParticipations.ToList();
			context.AppointmentParticipations.RemoveRange(userAppointmentParticipations);

			// User himself
			context.Users.Remove(user);

			return deletedEventNotificationInfos;
		}

		public EventNotificationInformation DeleteEventLocally(IDatabaseContext context, Event @event)
		{
			// Backup data relevant for sending notifications
			EventNotificationInformation notificationInformation = ExtractNotificationInformation(@event);

			// Participations for the event
			List<EventParticipation> participations = @event.EventParticipations.ToList();
			context.EventParticipations.RemoveRange(participations);

			// Appointments of the event
			List<Appointment> appointments = @event.Appointments.ToList();
			context.Appointments.RemoveRange(appointments);
			context.AppointmentParticipations.RemoveRange(appointments.SelectMany(a => a.AppointmentParticipations));

			// Invitations to the event
			context.EventInvitations.RemoveRange(@event.EventInvitations);

			// Event itself
			context.Events.Remove(@event);

			return notificationInformation;
		}

		public AppointmentNotificationInformation DeleteAppointmentLocally(IDatabaseContext context, Appointment appointment)
		{
			// Backup data relevant for sending notifications
			var notificationInformation = new AppointmentNotificationInformation(appointment.Id, appointment.StartTime,
				appointment.AppointmentParticipations.Select(ExtractNotificationInformation).ToList());

			List<AppointmentParticipation> participations = appointment.AppointmentParticipations.ToList();
			context.AppointmentParticipations.RemoveRange(participations);
			context.Appointments.Remove(appointment);

			return notificationInformation;
		}
	}
}
