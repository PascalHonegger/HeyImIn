using System;
using System.Threading.Tasks;
using HeyImIn.Database.Models;

namespace HeyImIn.MailNotifier.Impl
{
	public class NotificationService : INotificationService
	{
		public Task SendPasswordResetTokenAsync(Guid token, User recipient)
		{
			// TODO Implement
			return Task.CompletedTask;
		}
	}
}