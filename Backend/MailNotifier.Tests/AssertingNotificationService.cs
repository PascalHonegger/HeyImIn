using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier.Models;
using Xunit;

namespace HeyImIn.MailNotifier.Tests
{
	public class AssertingNotificationService : INotificationService
	{
		public virtual Task SendPasswordResetTokenAsync(Guid token, User recipient)
		{
			Assert.NotEqual(new Guid(), token);
			Assert.NotNull(recipient);

			return Task.CompletedTask;
		}

		public Task SendInvitationLinkAsync(List<(User user, EventInvitation invite)> userInvitations, List<(string email, EventInvitation invite)> newInvitations)
		{
			Assert.NotNull(userInvitations);

			foreach ((User user, EventInvitation invite) in userInvitations)
			{
				Assert.NotNull(user);
				Assert.NotNull(invite);
				Assert.NotNull(invite.Event);
				Assert.NotNull(invite.Event.Organizer);
			}

			Assert.NotNull(newInvitations);

			foreach ((string _, EventInvitation invite) in newInvitations)
			{
				Assert.NotNull(invite);
				Assert.NotNull(invite.Event);
				Assert.NotNull(invite.Event.Organizer);
			}

			return Task.CompletedTask;
		}

		public virtual Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change)
		{
			Assert.NotNull(@event);
			Assert.NotNull(@event.Organizer);
			Assert.NotNull(affectedUser);

			return Task.CompletedTask;
		}

		public virtual Task SendAndUpdateRemindersAsync(Appointment appointment)
		{
			Assert.NotNull(appointment);
			Assert.NotNull(appointment.Event);
			Assert.NotNull(appointment.Event.EventParticipations);
			foreach (EventParticipation eventEventParticipation in appointment.Event.EventParticipations)
			{
				Assert.NotNull(eventEventParticipation);
				Assert.NotNull(eventEventParticipation.Participant);
			}
			Assert.NotNull(appointment.AppointmentParticipations);
			foreach (AppointmentParticipation appointmentParticipation in appointment.AppointmentParticipations)
			{
				Assert.NotNull(appointmentParticipation);
				Assert.NotNull(appointmentParticipation.Participant);
				Assert.NotNull(appointmentParticipation.Appointment);
				Assert.NotNull(appointmentParticipation.Appointment.Event);
				Assert.NotNull(appointmentParticipation.Appointment.Event.EventParticipations);
			}

			return Task.CompletedTask;
		}

		public virtual Task SendAndUpdateSummariesAsync(Appointment appointment)
		{
			Assert.NotNull(appointment);
			Assert.NotNull(appointment.Event);
			Assert.NotNull(appointment.AppointmentParticipations);
			foreach (AppointmentParticipation appointmentParticipation in appointment.AppointmentParticipations)
			{
				Assert.NotNull(appointmentParticipation);
				Assert.NotNull(appointmentParticipation.Participant);
				Assert.NotNull(appointmentParticipation.Appointment);
				Assert.NotNull(appointmentParticipation.Appointment.Event);
				Assert.NotNull(appointmentParticipation.Appointment.Event.EventParticipations);
			}

			return Task.CompletedTask;
		}

		public virtual Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment)
		{
			Assert.NotNull(appointment);
			Assert.NotNull(appointment.Event);
			Assert.NotNull(appointment.AppointmentParticipations);
			foreach (AppointmentParticipation participation in appointment.AppointmentParticipations)
			{
				Assert.NotNull(participation);
				Assert.NotNull(participation.Participant);
			}

			return Task.CompletedTask;
		}

		public virtual Task NotifyAppointmentExplicitlyCanceledAsync(AppointmentNotificationInformation appointment, Event @event)
		{
			Assert.NotNull(appointment);
			Assert.NotNull(appointment.Participations);
			foreach (AppointmentParticipationNotificationInformation participation in appointment.Participations)
			{
				Assert.NotNull(participation);
				Assert.NotNull(participation.Participant);
			}

			Assert.NotNull(@event);

			return Task.CompletedTask;
		}

		public virtual Task NotifyEventDeletedAsync(EventNotificationInformation @event)
		{
			Assert.NotNull(@event);
			Assert.NotNull(@event.Title);
			Assert.NotNull(@event.Participations);
			foreach (UserNotificationInformation participation in @event.Participations)
			{
				Assert.NotNull(participation);
				Assert.NotNull(participation.Email);
				Assert.NotNull(participation.FullName);
			}

			return Task.CompletedTask;
		}

		public virtual Task NotifyEventUpdatedAsync(Event @event)
		{
			Assert.NotNull(@event);
			Assert.NotNull(@event.EventParticipations);
			foreach (EventParticipation participation in @event.EventParticipations)
			{
				Assert.NotNull(participation);
				Assert.NotNull(participation.Participant);
			}

			return Task.CompletedTask;
		}

		public virtual Task NotifyUnreadChatMessagesAsync(ChatMessagesNotificationInformation chatMessageInformation)
		{
			Assert.NotNull(chatMessageInformation);
			Assert.NotNull(chatMessageInformation.RelevantUserData);
			Assert.NotNull(chatMessageInformation.EventTitle);
			Assert.NotNull(chatMessageInformation.Messages);

			foreach (ChatMessageNotificationInformation chatMessage in chatMessageInformation.Messages)
			{
				Assert.NotNull(chatMessage);
				Assert.NotNull(chatMessage.Content);
			}

			return Task.CompletedTask;
		}
	}
}
