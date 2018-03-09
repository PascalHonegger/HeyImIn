using System;
using System.Threading.Tasks;
using HeyImIn.Database.Models;

namespace HeyImIn.MailNotifier
{
	/// <summary>
	///     A service which encapsulates the sending of an email
	///     Is responsible for the formatting and wording of the email
	/// </summary>
	public interface INotificationService
	{
		Task SendPasswordResetTokenAsync(Guid token, User recipient);
	}
}
