using System.Collections.Generic;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier.Models;

namespace HeyImIn.WebApplication.Services
{
	public interface IDeleteService
	{
		/// <summary>
		///     Removes the user, his organized events and all participations from the context
		/// </summary>
		/// <param name="context"></param>
		/// <param name="user">User to remove</param>
		/// <returns>Backed up data for sending notifications</returns>
		List<EventNotificationInformation> DeleteUserLocally(IDatabaseContext context, User user);

		/// <summary>
		///     Removes the event and all participations from the context
		/// </summary>
		/// <param name="context"></param>
		/// <param name="event">Event to remove</param>
		/// <returns>Backed up data for sending notifications</returns>
		EventNotificationInformation DeleteEventLocally(IDatabaseContext context, Event @event);

		/// <summary>
		///     Removes the appointment and all participations from the context
		/// </summary>
		/// <param name="context"></param>
		/// <param name="appointment">Appointment to remove</param>
		/// <returns>Backed up data for sending notifications</returns>
		AppointmentNotificationInformation DeleteAppointmentLocally(IDatabaseContext context, Appointment appointment);
	}
}
