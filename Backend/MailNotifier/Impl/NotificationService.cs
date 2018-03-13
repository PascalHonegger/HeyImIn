using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Models;
using log4net;

namespace HeyImIn.MailNotifier.Impl
{
	public class NotificationService : INotificationService
	{
		public NotificationService(IMailSender mailSender, string baseWebUrl)
		{
			_mailSender = mailSender;
			_baseWebUrl = baseWebUrl;
		}

		public async Task SendPasswordResetTokenAsync(Guid token, User recipient)
		{
			const string PasswordResetSubject = "Passwort zurücksetzen";
			string message = $@"Hallo {recipient.FullName}

Soeben wurde eine Zurücksetzung des Passworts für diese E-Mail-Adresse beantragt. Der Code dafür lautet:

{token}

Sie können diesen Code unter {_baseWebUrl}ResetPassword eingeben und Ihr Passwort zurücksetzen.";

			await _mailSender.SendMailAsync(recipient.Email, PasswordResetSubject, message);

			_log.InfoFormat("{0}(userId={1}, userEmail={2}): Sent password reset token", nameof(SendPasswordResetTokenAsync), recipient.Id, recipient.Email);
		}

		public async Task SendInvitationLinkAsync(List<(User user, EventInvitation invite)> userInvitations, List<(string email, EventInvitation invite)> newInvitations)
		{
			const string EventInviteSubject = "Sie wurden zu einem Event eingeladen";

			foreach ((User user, EventInvitation invite) in userInvitations)
			{
				string personalizedMessage = $@"Hallo {user.FullName}

Sie wurden von {invite.Event.Organizer.FullName} zum Event ""{invite.Event.Title}"" eingeladen.

Sie können diese Einladung unter {_baseWebUrl}AcceptInvitation/{invite.Token} annehmen.";


				await _mailSender.SendMailAsync(user.Email, EventInviteSubject, personalizedMessage);
			}

			foreach ((string email, EventInvitation invite) in newInvitations)
			{
				string generalizedMessage = $@"Hallo

Sie wurden von {invite.Event.Organizer.FullName} zum Event ""{invite.Event.Title}"" eingeladen.

Sie können diese Einladung unter {_baseWebUrl}AcceptInvitation/{invite.Token} annehmen.";


				await _mailSender.SendMailAsync(email, EventInviteSubject, generalizedMessage);
			}

			_log.InfoFormat("{0}(): Sent {1} invites", nameof(SendPasswordResetTokenAsync), userInvitations.Count + newInvitations.Count);
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

		private readonly IMailSender _mailSender;

		/// <summary>
		///     The url being referd to within emails
		/// </summary>
		private readonly string _baseWebUrl;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
