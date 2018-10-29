using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier.Models;
using HeyImIn.Shared;
using Microsoft.Extensions.Logging;

namespace HeyImIn.MailNotifier.Impl
{
	public class NotificationService : INotificationService
	{
		public NotificationService(IMailSender mailSender, ISessionService sessionService, HeyImInConfiguration configuration, ILogger<NotificationService> logger)
		{
			_mailSender = mailSender;
			_sessionService = sessionService;
			_logger = logger;

			_baseWebUrl = configuration.FrontendBaseUrl;

			try
			{
				_mailTimeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.MailTimeZoneName);
			}
			catch (Exception e)
			{
				_logger.LogError("{0}(): Configured time zone '{1}' was not valid, error={2}", nameof(NotificationService), configuration.MailTimeZoneName, e);

				_mailTimeZone = TimeZoneInfo.Utc;
			}
		}

		/// <summary>
		///     Returns all participations which pass the provided filters and were accepted
		/// </summary>
		private static List<AppointmentParticipation> FilterAcceptedParticipations(IEnumerable<AppointmentParticipation> participations,
																				   Func<AppointmentParticipation, bool> participationFilter,
																				   Func<EventParticipation, bool> eventFilter)
		{
			return participations
				.Where(p => p.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted)
				.Where(participationFilter)
				.Where(p => eventFilter(p.Appointment.Event.EventParticipations.First(e => e.Participant == p.Participant)))
				.ToList();
		}

		/// <summary>
		///     Returns all participations which pass the provided filters
		///     If no answer exists, Automatically adds appointment participations with the answer null
		/// </summary>
		private static List<AppointmentParticipation> FilterAndAppendAllParticipations(Appointment appointment,
																					   Func<AppointmentParticipation, bool> participationFilter,
																					   Func<EventParticipation, bool> eventFilter)
		{
			// First create the appointment participations for users which have no participation yet
			// A appointment participation is required to track the sent emails
			List<int> participantIds = appointment.AppointmentParticipations.Select(p => p.ParticipantId).ToList();

			foreach (EventParticipation eventParticipationWithoutAppointmentParticipation in appointment.Event.EventParticipations.Where(e => !participantIds.Contains(e.ParticipantId)))
			{
				var newAppointmentParticipation = new AppointmentParticipation
				{
					Appointment = appointment,
					Participant = eventParticipationWithoutAppointmentParticipation.Participant,
					AppointmentParticipationAnswer = null,
					SentReminder = false,
					SentSummary = false
				};

				appointment.AppointmentParticipations.Add(newAppointmentParticipation);
			}

			return appointment.AppointmentParticipations
				.Where(participationFilter)
				.Where(p => eventFilter(p.Appointment.Event.EventParticipations.First(e => e.Participant == p.Participant)))
				.ToList();
		}

		private async Task<string> CreateAuthTokenSuffixAsync(int userId)
		{
			Guid createdSessionToken = await _sessionService.CreateSessionAsync(userId, false);

			return $"?authToken={createdSessionToken}";
		}

		private DateTime TargetTimeZone(DateTime utcDateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _mailTimeZone);
		}

		private static (int count, string formattedString) ParticipationsList(IEnumerable<AppointmentParticipation> allParticipations)
		{
			List<string> acceptedParticipantsList = allParticipations
				.Where(a => a.AppointmentParticipationAnswer == AppointmentParticipationAnswer.Accepted)
				.Select(a => $"- {a.Participant.FullName}")
				.ToList();

			return (acceptedParticipantsList.Count, string.Join(Environment.NewLine, acceptedParticipantsList));
		}
		private readonly IMailSender _mailSender;

		/// <summary>
		///     The url being referred to within emails
		/// </summary>
		private readonly string _baseWebUrl;

		private readonly ISessionService _sessionService;
		private readonly TimeZoneInfo _mailTimeZone;

		private readonly ILogger<NotificationService> _logger;

		#region No data deleted

		public async Task SendPasswordResetTokenAsync(Guid token, User recipient)
		{
			const string PasswordResetSubject = "Passwort zurücksetzen";
			string message = $@"Hallo {recipient.FullName}

Soeben wurde eine Zurücksetzung des Passworts für diese E-Mail-Adresse beantragt. Der Code dafür lautet:

{token}

Sie können diesen Code unter {_baseWebUrl}ResetPassword eingeben und Ihr Passwort zurücksetzen.";

			await _mailSender.SendMailAsync(recipient.Email, PasswordResetSubject, message);

			_logger.LogInformation("{0}(userId={1}, userEmail={2}): Sent password reset token", nameof(SendPasswordResetTokenAsync), recipient.Id, recipient.Email);
		}

		public async Task SendInvitationLinkAsync(List<(User user, EventInvitation invite)> userInvitations, List<(string email, EventInvitation invite)> newInvitations)
		{
			const string EventInviteSubject = "Sie wurden zu einem Event eingeladen";

			foreach ((User user, EventInvitation invite) in userInvitations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(user.Id);

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

			_logger.LogInformation("{0}(): Sent {1} invites", nameof(SendInvitationLinkAsync), userInvitations.Count + newInvitations.Count);
		}

		public async Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change)
		{
			const string OrganizerUpdatedUserSubject = "Ein Organisator hat Änderungen für Sie vorgenommen";

			string authTokenSuffix = await CreateAuthTokenSuffixAsync(affectedUser.Id);

			string message = $@"Hallo {affectedUser.FullName}

{@event.Organizer.FullName}, der Organisator des Events '{@event.Title}', hat folgende Änderungen für Sie vorgenommen:

{change}

Sie können den betroffenen Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


			await _mailSender.SendMailAsync(affectedUser.Email, OrganizerUpdatedUserSubject, message);

			_logger.LogInformation("{0}(): Sent notification as a organizer of event {1} changed something about the user {2}", nameof(NotifyOrganizerUpdatedUserInfoAsync), @event.Id, affectedUser.Id);
		}

		public async Task SendAndUpdateRemindersAsync(Appointment appointment)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string reminderSubject = $"Reminder Event '{appointment.Event.Title}' zum Termin am {startTime:g}";

			List<AppointmentParticipation> participationsAwaitingReminder = FilterAndAppendAllParticipations(appointment, p => !p.SentReminder, e => e.SendReminderEmail);

			foreach (AppointmentParticipation appointmentParticipation in participationsAwaitingReminder)
			{
				string state;
				switch (appointmentParticipation.AppointmentParticipationAnswer)
				{
					case AppointmentParticipationAnswer.Accepted:
						state = "zugesagt";
						break;
					case AppointmentParticipationAnswer.Declined:
						state = "abgesagt";
						break;
					case null:
						state = "noch keine Antwort gegeben";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				string message = await ComposeMessageAsync(appointmentParticipation.Participant, state);

				await _mailSender.SendMailAsync(appointmentParticipation.Participant.Email, reminderSubject, message);

				appointmentParticipation.SentReminder = true;
			}

			_logger.LogInformation("{0}(): Sent {1} reminders for appointment {2}", nameof(SendAndUpdateRemindersAsync), participationsAwaitingReminder.Count, appointment.Id);

			async Task<string> ComposeMessageAsync(User participant, string state)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participant.Id);

				return $@"Hallo {participant.FullName}

Sie haben für den Termin am {startTime:g} zum Event '{appointment.Event.Title}' {state}.

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";
			}
		}

		public async Task SendAndUpdateSummariesAsync(Appointment appointment)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string summarySubject = $"Zusammenfassung Event '{appointment.Event.Title}' zum Termin am {startTime:g}";

			(int count, string formattedString) = ParticipationsList(appointment.AppointmentParticipations);

			string messageBody = $@"Die Teilnehmer treffen sich am {startTime:g} am Treffpunkt '{appointment.Event.MeetingPlace}'

Die finale Teilnehmerliste der {count} Zusagen:

{formattedString}";

			List<AppointmentParticipation> participationsAwaitingSummary = FilterAcceptedParticipations(appointment.AppointmentParticipations, p => !p.SentSummary, e => e.SendSummaryEmail);

			foreach (AppointmentParticipation participation in participationsAwaitingSummary)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant.Id);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";

				await _mailSender.SendMailAsync(participation.Participant.Email, summarySubject, message);

				participation.SentSummary = true;
			}

			_logger.LogInformation("{0}(): Sent {1} summaries for appointment {2}", nameof(SendAndUpdateSummariesAsync), participationsAwaitingSummary.Count, appointment.Id);
		}

		public async Task NotifyEventUpdatedAsync(Event @event)
		{
			string updatedSubject = $"Der Event '{@event.Title}' wurde editiert";

			string messageBody = $"Die Informationen zum Event '{@event.Title}', an welchem Sie teilnehmen, wurden aktualisiert.";

			foreach (EventParticipation participation in @event.EventParticipations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant.Id);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, updatedSubject, message);
			}

			_logger.LogInformation("{0}(): Sent {1} notifications about a change of the event {2}", nameof(NotifyEventUpdatedAsync), @event.EventParticipations.Count, @event.Id);
		}

		public async Task NotifyOrganizerChangedAsync(Event @event)
		{
			string updatedSubject = $"Der Event '{@event.Title}' wurde an {@event.Organizer.FullName} übergeben";

			string messageBody = $"Der Event '{@event.Title}' wird neu von {@event.Organizer.FullName} organisiert.";

			foreach (EventParticipation participation in @event.EventParticipations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant.Id);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, updatedSubject, message);
			}

			_logger.LogInformation("{0}(): Sent {1} notifications about the new organizer of the event {2}", nameof(NotifyEventUpdatedAsync), @event.EventParticipations.Count, @event.Id);
		}

		public async Task NotifyUnreadChatMessagesAsync(ChatMessagesNotificationInformation chatMessagesInformation)
		{
			string unreadMessagesSubject = $"Ungelesene Nachrichten im Event '{chatMessagesInformation.EventTitle}'";

			var messageBodyBuilder = new StringBuilder();
			messageBodyBuilder.AppendLine($"Folgende Nachrichten wurden im Event '{chatMessagesInformation.EventTitle}' versendet:");
			messageBodyBuilder.AppendLine();

			const string ChatMessageSeparator = "------------------------------";

			messageBodyBuilder.AppendLine(ChatMessageSeparator);

			foreach (ChatMessageNotificationInformation chatMessage in chatMessagesInformation.Messages)
			{
				string authorName = chatMessagesInformation.RelevantUserData.First(u => u.id == chatMessage.AuthorId).fullName;

				DateTime sentDate = TargetTimeZone(chatMessage.SentDate);

				messageBodyBuilder.Append(
$@"
*{sentDate:g} – {authorName}*

{chatMessage.Content}

{ChatMessageSeparator}
");
			}

			string authTokenSuffix = await CreateAuthTokenSuffixAsync(chatMessagesInformation.ParticipantId);

			(_, string participantName, string participantEmail) = chatMessagesInformation.RelevantUserData.First(u => u.id == chatMessagesInformation.ParticipantId);

			string message = $@"Hallo {participantName}

{messageBodyBuilder}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{chatMessagesInformation.EventId}{authTokenSuffix} ansehen.";


			await _mailSender.SendMailAsync(participantEmail, unreadMessagesSubject, message);

			_logger.LogInformation("{0}(): Sent {1} missed chat messages of the event {2}", nameof(NotifyEventUpdatedAsync), chatMessagesInformation.Messages.Count, chatMessagesInformation.EventId);
		}

		public async Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment)
		{
			if (appointment.StartTime <= DateTime.UtcNow)
			{
				return;
			}

			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string lastMinuteChangeSubject = $"Kurzfristige Änderung zum Termin am {startTime:g}";

			(int count, string formattedString) = ParticipationsList(appointment.AppointmentParticipations);

			string messageBody = $@"Die Teilnehmer des Events '{appointment.Event.Title}' haben sich geändert. Die aktualisierte Teilnehmerliste der {count} Zusagen:

{formattedString}";

			List<AppointmentParticipation> participationsAlreadyGotSummary = FilterAcceptedParticipations(appointment.AppointmentParticipations, p => p.SentSummary, e => e.SendLastMinuteChangesEmail);

			foreach (AppointmentParticipation participation in participationsAlreadyGotSummary)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant.Id);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{appointment.Event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, lastMinuteChangeSubject, message);
			}

			_logger.LogInformation("{0}(): Sent {1} updated summaries for appointment {2}", nameof(SendLastMinuteChangeIfRequiredAsync), participationsAlreadyGotSummary.Count, appointment.Id);
		}

		#endregion

		#region Data possibly deleted

		public async Task NotifyAppointmentExplicitlyCanceledAsync(AppointmentNotificationInformation appointment, Event @event)
		{
			DateTime startTime = TargetTimeZone(appointment.StartTime);

			string summarySubject = $"Termin am {startTime:g} zum Event '{@event.Title}' abgesagt";

			string messageBody = $"Sie haben dem Termin ({startTime:g}) zugesagt, welcher vom Organisator abgesagt wurde.";

			List<AppointmentParticipationNotificationInformation> acceptedParticipations = appointment.Participations.Where(p => p.Answer == AppointmentParticipationAnswer.Accepted).ToList();

			foreach (AppointmentParticipationNotificationInformation participation in acceptedParticipations)
			{
				string authTokenSuffix = await CreateAuthTokenSuffixAsync(participation.Participant.Id);

				string message = $@"Hallo {participation.Participant.FullName}

{messageBody}

Sie können weitere Details zum Event unter {_baseWebUrl}ViewEvent/{@event.Id}{authTokenSuffix} ansehen.";


				await _mailSender.SendMailAsync(participation.Participant.Email, summarySubject, message);
			}

			_logger.LogInformation("{0}(): Sent {1} notifications about cancelation of appointment {2}", nameof(NotifyAppointmentExplicitlyCanceledAsync), acceptedParticipations.Count, appointment.Id);
		}

		public async Task NotifyEventDeletedAsync(EventNotificationInformation @event)
		{
			string deletedSubject = $"Der Event '{@event.Title}' wurde gelöscht";

			string messageBody = $"Der Event '{@event.Title}', an welchem Sie teilgenommen haben, wurde zusammen mit allen Terminen gelöscht.";

			foreach (UserNotificationInformation participant in @event.Participations)
			{
				string message = $@"Hallo {participant.FullName}

{messageBody}";


				await _mailSender.SendMailAsync(participant.Email, deletedSubject, message);
			}

			_logger.LogInformation("{0}(): Sent {1} notifications about the deletion of event {2} with the title '{3}'", nameof(NotifyEventDeletedAsync), @event.Participations.Count, @event.Id, @event.Title);
		}

		#endregion
	}
}
