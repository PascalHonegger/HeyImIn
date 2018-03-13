using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeyImIn.Database.Models;

namespace HeyImIn.MailNotifier.Impl
{
	public class NotificationService : INotificationService
	{
		public Task SendPasswordResetTokenAsync(Guid token, User recipient)
		{
			throw new NotImplementedException();
		}

		public Task SendInvitationLinkAsync(List<(string email, EventInvitation invite)> invitations)
		{
			throw new NotImplementedException();
		}

		public Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change)
		{
			throw new NotImplementedException();
		}

		public Task SendSummaryIfRequiredAsync(Appointment appointment)
		{
			throw new NotImplementedException();
		}

		public Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment)
		{
			TimeSpan summaryTimeSpan = TimeSpan.FromHours(appointment.Event.SummaryTimeWindowInHours);

			if ((appointment.StartTime >= DateTime.UtcNow) && (appointment.StartTime - summaryTimeSpan <= DateTime.UtcNow))
			{
				// Send
			}

			throw new NotImplementedException();
		}

		public Task SendReminderIfRequiredAsync(Appointment appointment)
		{
			throw new NotImplementedException();
		}

		public Task NotifyAppointmentExplicitlyCanceledAsync(Appointment appointment)
		{
			throw new NotImplementedException();
		}

		public Task NotifyEventDeletedAsync(Event @event)
		{
			throw new NotImplementedException();
		}

		public Task NotifyEventUpdatedAsync(Event @event)
		{
			throw new NotImplementedException();
		}
	}
}