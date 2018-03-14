﻿using System;
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
	public class CronSendNotificationsService : ICronService
	{
		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		public CronSendNotificationsService(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		public async Task RunAsync()
		{
			_log.DebugFormat("{0}(): Start running notification Cron", nameof(RunAsync));

			using (IDatabaseContext context = _getDatabaseContext())
			{
				List<Appointment> appointmentsWithPossibleReminders = await context.Appointments
					.Where(a => (a.StartTime >= DateTime.UtcNow) && (DbFunctions.DiffHours(a.StartTime, DateTime.UtcNow) <= a.Event.ReminderTimeWindowInHours))
					.ToListAsync();

				List<Appointment> appointmentsWithPossibleSummaries = await context.Appointments
					.Where(a => (a.StartTime >= DateTime.UtcNow) && (DbFunctions.DiffHours(a.StartTime, DateTime.UtcNow) <= a.Event.SummaryTimeWindowInHours))
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

			_log.DebugFormat("{0}(): Finished running notification Cron", nameof(RunAsync));
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}