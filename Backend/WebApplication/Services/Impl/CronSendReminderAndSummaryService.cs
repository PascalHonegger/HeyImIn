using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Services.Impl
{
	/// <summary>
	///     Sends notifications for reminders and summaries, which are dependent on the current time
	/// </summary>
	public class CronSendReminderAndSummaryService : ICronService
	{
		public CronSendReminderAndSummaryService(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		public async Task RunAsync(CancellationToken token)
		{
			IDatabaseContext context = _getDatabaseContext();
			List<Appointment> appointmentsWithPossibleReminders = await GetFutureAppointmentsAsync(a => a.StartTime.AddHours(-a.Event.ReminderTimeWindowInHours) <= DateTime.UtcNow);
			List<Appointment> appointmentsWithPossibleSummaries = await GetFutureAppointmentsAsync(a => a.StartTime.AddHours(-a.Event.SummaryTimeWindowInHours) <= DateTime.UtcNow);

			foreach (Appointment appointmentsWithPossibleReminder in appointmentsWithPossibleReminders)
			{
				await _notificationService.SendAndUpdateRemindersAsync(appointmentsWithPossibleReminder);
			}

			foreach (Appointment appointmentsWithPossibleSummary in appointmentsWithPossibleSummaries)
			{
				await _notificationService.SendAndUpdateSummariesAsync(appointmentsWithPossibleSummary);
			}

			// Save sent reminders & summaries
			await context.SaveChangesAsync(token);

			async Task<List<Appointment>> GetFutureAppointmentsAsync(Expression<Func<Appointment, bool>> additionalAppointmentFilter)
			{
				return await context.Appointments
					.Include(a => a.Event)
						.ThenInclude(e => e.EventParticipations)
					.Include(a => a.AppointmentParticipations)
						.ThenInclude(ap => ap.Participant)
					.Where(a => a.StartTime >= DateTime.UtcNow)
					.Where(additionalAppointmentFilter)
					.ToListAsync(token);
			}
		}

		public string DescriptiveName { get; } = "SendReminderAndSummaryCron";

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;
	}
}
