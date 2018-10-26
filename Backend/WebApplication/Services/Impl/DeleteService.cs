using System.Collections.Generic;
using System.Linq;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier.Models;

namespace HeyImIn.WebApplication.Services.Impl
{
	public class DeleteService : IDeleteService
	{
		public List<EventNotificationInformation> DeleteUserLocally(IDatabaseContext context, User user)
		{
			// Backup data relevant for sending notifications
			var deletedEventNotificationInfos = new List<EventNotificationInformation>();

			foreach (Event organizedEvent in user.OrganizedEvents.ToList())
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
			context.AppointmentParticipations.RemoveRange(user.AppointmentParticipations);

			// ChatMessages belonging to the event
			context.ChatMessages.RemoveRange(user.ChatMessages);

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

			// Appointments of the even
			foreach (Appointment eventAppointment in @event.Appointments.ToList())
			{
				// Ignore notification information as we don't send appointment canceled if the whole event gets deleted
				DeleteAppointmentLocally(context, eventAppointment);
			}

			// Invitations to the event
			context.EventInvitations.RemoveRange(@event.EventInvitations);

			// ChatMessages belonging to the event
			context.ChatMessages.RemoveRange(@event.ChatMessages);

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
	}
}
