using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using log4net;

namespace HeyImIn.MailNotifier.Impl
{
	public class NotificationService : INotificationService
	{
		public NotificationService(IMailSender mailSender, ISessionService sessionService, string baseWebUrl, string mailTimeZoneName)
		{
			_mailSender = mailSender;
			_sessionService = sessionService;
			_baseWebUrl = baseWebUrl;

			try
			{
				_mailTimeZone = TimeZoneInfo.FindSystemTimeZoneById(mailTimeZoneName);
			}
			catch (Exception e)
			{
				_log.ErrorFormat("{0}(): Configured time zone '{1}' was not valid, error={2}", nameof(NotificationService), mailTimeZoneName, e);

				_mailTimeZone = TimeZoneInfo.Utc;
			}
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
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(user);

				string personalizedMessage = $@"Hallo {user.FullName}

Sie wurden von {invite.Event.Organizer.FullName} zum Event '{invite.Event.Title}' eingeladen.

Sie können diese Einladung unter {_baseWebUrl}AcceptInvitation/{invite.Token}{authTokenSuffix} annehmen.";


				await _mailSender.SendMailAsync(user.Email, EventInviteSubject, personalizedMessage);
			}

			foreach ((string email, EventInvitation invite) in newInvitations)
			{
				string generalizedMessage = $@"Hallo

Sie wurden von {invite.Event.Organizer.FullName} zum Event '{invite.Event.Title}' eingeladen.

Sie können diese Einladung unter {_baseWebUrl}AcceptInvitation/{invite.Token} annehmen.";


				await _mailSender.SendMailAsync(email, EventInviteSubject, generalizedMessage);
			}

			_log.InfoFormat("{0}(): Sent {1} invites", nameof(SendInvitationLinkAsync), userInvitations.Count + newInvitations.Count);
		}

		public async Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change)
		{
			const string OrganizerUpdatedUserSubject = "Ein Organisator hat Änderungen für Sie vorgenommen";

			string authTokenSuffix = await CreateAuthTokenSuffixAsync(affectedUser);

			string message = $@"Hallo {affectedUser.FullName}

{@event.Organizer.FullName}, der Organisator des Events '{@event.Title}', hat folgende Änderungen für Sie vorgenommen:

{change}

Sie können den betroffenen Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


			await _mailSender.SendMailAsync(affectedUser.Email, OrganizerUpdatedUserSubject, message);

			_log.InfoFormat("{0}(): Sent notification as a organizer of event {1} changed something about the user {2}", nameof(NotifyOrganizerUpdatedUserInfoAsync), @event.Id, affectedUser.Id);
		}

		public async Task SendAndUpdateRemindersAsync(Appointment appointment)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string reminderSubject = $"Reminder Event '{appointment.Event.Title}' zum Termin am {startTime:g}";

			List<AppointmentParticipation> participationsAwaitingReminder = FilterParticipations(appointment.AppointmentParticipations, p => !p.SentReminder, e => e.SendReminderEmail);

			foreach (AppointmentParticipation participation in participationsAwaitingReminder)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant);

				string message = $@"Hallo {participation.Participant.FullName}

Sie haben dem Termin am {startTime:g} zum Event '{appointment.Event.Title}' zugesagt.

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, reminderSubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} reminders for appointment {2}", nameof(SendAndUpdateRemindersAsync), participationsAwaitingReminder.Count, appointment.Id);
		}

		public async Task SendAndUpdateSummariesAsync(Appointment appointment)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string summarySubject = $"Zusammenfassung Event '{appointment.Event.Title}' zum Termin am {startTime:g}";

			string participants = ParticipationsList(appointment.AppointmentParticipations);

			string messageBody = $@"Die Teilnehmer treffen sich am {startTime:g} am Treffpunkt '{appointment.Event.MeetingPlace}'

Die finale Teilnehmerliste:
{participants}";

			List<AppointmentParticipation> participationsAwaitingSummary = FilterParticipations(appointment.AppointmentParticipations, p => !p.SentSummary, e => e.SendSummaryEmail);

			foreach (AppointmentParticipation participation in participationsAwaitingSummary)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, summarySubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} summaries for appointment {2}", nameof(SendAndUpdateSummariesAsync), participationsAwaitingSummary.Count, appointment.Id);
		}

		public async Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment)
		{
			if (appointment.StartTime <= DateTime.UtcNow)
			{
				return;
			}

			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string lastMinuteChangeSubject = $"Kurzfristige Änderung zum Termin am {startTime:g}";

			string participants = ParticipationsList(appointment.AppointmentParticipations);

			string messageBody = $@"Die Teilnehmer des Events '{appointment.Event.Title}' haben sich geändert. Die aktualisierte Teilnehmerliste:
{participants}";

			List<AppointmentParticipation> participationsAwaitingSummary = FilterParticipations(appointment.AppointmentParticipations, p => p.SentSummary, e => e.SendLastMinuteChangesEmail);

			foreach (AppointmentParticipation participation in participationsAwaitingSummary)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, lastMinuteChangeSubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} summaries for appointment {2}", nameof(SendAndUpdateSummariesAsync), participationsAwaitingSummary.Count, appointment.Id);
		}

		public async Task NotifyAppointmentExplicitlyCanceledAsync(Appointment appointment)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string summarySubject = $"Termin am {startTime:g} zum Event '{appointment.Event.Title}' abgesagt";

			string messageBody = $"Sie haben dem Termin ({startTime:g}) zugesagt, welcher vom Organisator abgesagt wurde.";

			List<AppointmentParticipation> acceptedParticipations = appointment.AppointmentParticipations.Where(p => p.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted).ToList();

			foreach (AppointmentParticipation participation in acceptedParticipations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, summarySubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} notifications about cancelation of appointment {2}", nameof(SendAndUpdateSummariesAsync), acceptedParticipations.Count, appointment.Id);
		}

		public async Task NotifyEventDeletedAsync(Event @event)
		{
			string deletedSubject = $"Der Event '{@event.Title}' wurde gelöscht";

			string messageBody = $"Der Event '{@event.Title}', an welchem Sie teilgenommen haben, wurde zusammen mit allen Terminen gelöscht.";

			foreach (EventParticipation participation in @event.EventParticipations)
			{
				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}";


				await _mailSender.SendMailAsync(participation.Participant.Email, deletedSubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} notifications about the deletion of the event {2}", nameof(NotifyEventDeletedAsync), @event.EventParticipations.Count, @event.Id);
		}

		public async Task NotifyEventUpdatedAsync(Event @event)
		{
			string updatedSubject = $"Der Event '{@event.Title}' wurde editiert";

			string messageBody = $"Die Informationen zum Event '{@event.Title}', an welchem Sie teilnehmen, wurden aktualisiert.";

			foreach (EventParticipation participation in @event.EventParticipations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, updatedSubject, message);
			}

			_log.InfoFormat("{0}(): Sent {1} notifications about a change of the event {2}", nameof(NotifyEventUpdatedAsync), @event.EventParticipations.Count, @event.Id);
		}

		/// <summary>
		///     Returns all participations which pass the provided filters and were accepted
		/// </summary>
		private List<AppointmentParticipation> FilterParticipations(IEnumerable<AppointmentParticipation> participations,
																	Func<AppointmentParticipation, bool> participationFilter,
																	Func<EventParticipation, bool> eventFilter)
		{
			return participations
				.Where(p => p.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted)
				.Where(participationFilter)
				.Where(p => eventFilter(p.Appointment.Event.EventParticipations.First(e => e.Participant == p.Participant)))
				.ToList();
		}

		private async Task<string> CreateAuthTokenSuffixAsync(User user)
		{
			Guid createdSessionToken = await _sessionService.CreateSessionAsync(user.Id, false);

			return $"?authToken={createdSessionToken}";
		}

		private DateTime TargetTimeZone(DateTime utcDateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _mailTimeZone);
		}

		private static string ParticipationsList(IEnumerable<AppointmentParticipation> allParticipations)
		{
			return string.Join(Environment.NewLine, allParticipations
				.Where(a => a.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted)
				.Select(a => a.Participant.FullName));
		}

		private readonly IMailSender _mailSender;

		/// <summary>
		///     The url being referd to within emails
		/// </summary>
		private readonly string _baseWebUrl;

		private readonly ISessionService _sessionService;
		private readonly TimeZoneInfo _mailTimeZone;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
