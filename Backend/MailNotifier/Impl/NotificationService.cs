using System;
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

		public Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change)
		{
			throw new NotImplementedException();
		}

		public Task SendSummaryAsync(Event @event)
		{
			throw new NotImplementedException();
		}

		public Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment)
		{
			TimeSpan summaryTimeSpan = TimeSpan.FromHours(appointment.Event.SummaryTimeWindowInHours);

			if (appointment.StartTime - summaryTimeSpan <= DateTime.UtcNow)
			{
				// Send
			}

			throw new NotImplementedException();
		}

		public Task NotifyAppointmentDeletedAsync(Appointment appointment)
		{
			throw new NotImplementedException();
		}
	}
}