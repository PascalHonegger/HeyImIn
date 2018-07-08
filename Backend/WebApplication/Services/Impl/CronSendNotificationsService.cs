using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Services.Impl
{
	/// <summary>
	///     Sends time based notifications like reminders and summaries
	/// </summary>
	public class CronSendNotificationsService : ICronService
	{
		public CronSendNotificationsService(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		public async Task RunAsync()
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				List<Appointment> appointmentsWithPossibleReminders = await GetFutureAppointmentsAsync(context, a => a.StartTime.AddHours(-a.Event.ReminderTimeWindowInHours) <= DateTime.UtcNow);
				List<Appointment> appointmentsWithPossibleSummaries = await GetFutureAppointmentsAsync(context, a => a.StartTime.AddHours(-a.Event.SummaryTimeWindowInHours) <= DateTime.UtcNow);

				foreach (Appointment appointmentsWithPossibleReminder in appointmentsWithPossibleReminders)
				{
					await _notificationService.SendAndUpdateRemindersAsync(appointmentsWithPossibleReminder);
				}

				foreach (Appointment appointmentsWithPossibleSummary in appointmentsWithPossibleSummaries)
				{
					await _notificationService.SendAndUpdateSummariesAsync(appointmentsWithPossibleSummary);
				}

				// Save sent reminders & summaries
				await context.SaveChangesAsync();
			}

			async Task<List<Appointment>> GetFutureAppointmentsAsync(IDatabaseContext context, Expression<Func<Appointment, bool>> additionalAppointmentFilter)
			{
				return await context.Appointments
					.Include(a => a.Event)
						.ThenInclude(e => e.EventParticipations)
					.Include(a => a.AppointmentParticipations)
						.ThenInclude(ap => ap.Participant)
					.Where(a => a.StartTime >= DateTime.UtcNow)
					.Where(additionalAppointmentFilter)
					.ToListAsync();
			}
		}

		public string DescriptiveName { get; } = "SendNotificationCron";

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;
	}
}
