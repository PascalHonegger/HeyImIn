using System;
using System.Collections.Generic;
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
		/// <summary>
		///     Sends the provided token to the user, informing him about his password reset request
		/// </summary>
		/// <param name="token">Reset code to send</param>
		/// <param name="recipient">User who should receive the code</param>
		Task SendPasswordResetTokenAsync(Guid token, User recipient);

		/// <summary>
		///     Sends an invite link for each provided invitation
		/// </summary>
		/// <param name="userInvitations">Existing users</param>
		/// <param name="newInvitations">New users</param>
		Task SendInvitationLinkAsync(List<(User user, EventInvitation invite)> userInvitations, List<(string email, EventInvitation invite)> newInvitations);

		/// <summary>
		///     Sends a notifications after the organizer of an event changed some data for a user
		///     E.g. organizer removes a user
		/// </summary>
		/// <param name="event">Affected event, a link to this event will be included</param>
		/// <param name="affectedUser">The user who got updated by the organizer</param>
		/// <param name="change">The change that happened (E.g. "Organizer removed you from event")</param>
		Task NotifyOrganizerUpdatedUserInfoAsync(Event @event, User affectedUser, string change);

		/// <summary>
		///     Sends a reminder to all users who haven't received one yet
		///     Updates the appointment object -> Execute save() after this method was called
		/// </summary>
		/// <param name="appointment">Only users who accepted this appointment will get reminders</param>
		Task SendAndUpdateRemindersAsync(Appointment appointment);

		/// <summary>
		///     Sends a summary to all users who haven't recieved one yet
		///     Updates the appointment object -> Execute save() after this method was called
		/// </summary>
		/// <param name="appointment">Appointment for which to send summaries</param>
		Task SendAndUpdateSummariesAsync(Appointment appointment);

		/// <summary>
		///     Sends an updated summary to all users who have already received a summary
		/// </summary>
		/// <param name="appointment">Only users who accepted this appointment will get updated summaries</param>
		Task SendLastMinuteChangeIfRequiredAsync(Appointment appointment);

		/// <summary>
		///     Sends a notification that a specific appointment got canceled
		///     Only informs users who have accepted the appointment
		///     Is NOT called when a whole event gets deleted, as every user gets informed about that by
		///     <see cref="NotifyEventDeletedAsync" />
		/// </summary>
		/// <param name="appointmentTime">Time which was canceled</param>
		/// <param name="participations">Only users who accepted this appointment will be notified</param>
		/// <param name="event">Event the appointment was part of</param>
		Task NotifyAppointmentExplicitlyCanceledAsync(DateTime appointmentTime, IEnumerable<AppointmentParticipation> participations, Event @event);

		/// <summary>
		///     Sends a notification to all users who are participating in an event
		///     This happens either because the organizer deleted himself or a single event explicitly
		/// </summary>
		/// <param name="eventTitle">Event which got deleted</param>
		/// <param name="participations">Participants to inform</param>
		Task NotifyEventDeletedAsync(string eventTitle, IList<User> participations);

		/// <summary>
		///     Sends a notification to all users who are participating in an event
		///     This happens because the organizer changed some general information about the event
		///     E.g. changed the description or meeting place
		/// </summary>
		/// <param name="event">Event which got updated</param>
		Task NotifyEventUpdatedAsync(Event @event);
	}
}
