using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using log4net;

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
				List<Appointment> appointmentsWithPossibleReminders = await context.Appointments
					.Where(a => (a.StartTime >= DateTime.UtcNow) && (DbFunctions.AddHours(a.StartTime, -a.Event.ReminderTimeWindowInHours) <= DateTime.UtcNow))
					.ToListAsync();

				List<Appointment> appointmentsWithPossibleSummaries = await context.Appointments
					.Where(a => (a.StartTime >= DateTime.UtcNow) && (DbFunctions.AddHours(a.StartTime, -a.Event.SummaryTimeWindowInHours) <= DateTime.UtcNow))
					.ToListAsync();

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
		}

		public string DescriptiveName { get; } = "SendNotificationCron";

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
