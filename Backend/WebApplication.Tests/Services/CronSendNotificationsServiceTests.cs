using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared.Tests;
using HeyImIn.WebApplication.Services.Impl;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Services
{
	public class CronSendNotificationsServiceTests : TestBase
	{
		public CronSendNotificationsServiceTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task RunAsync_GivenFutureAppointments_SendsReminder()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Arrange
			Appointment appointment = await CreateTestDataAsync(getContext, DateTime.UtcNow + TimeSpan.FromHours(ReminderTimeWindowInHours));

			// Act
			(CronSendNotificationsService service, Mock<AssertingNotificationService> notificationServiceMock) = CreateService(getContext);
			notificationServiceMock.Setup(n => n.SendAndUpdateRemindersAsync(It.Is<Appointment>(a => a.Id == appointment.Id))).CallBase();
			await service.RunAsync();

			// Assert
			notificationServiceMock.Verify(n => n.SendAndUpdateRemindersAsync(It.Is<Appointment>(a => a.Id == appointment.Id)), Times.Once);
		}

		[Fact]
		public async Task RunAsync_GivenFutureAppointments_SendsSummaryAndReminder()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Arrange
			Appointment appointment = await CreateTestDataAsync(getContext, DateTime.UtcNow + TimeSpan.FromHours(SummaryTimeWindowInHours));

			// Act
			(CronSendNotificationsService service, Mock<AssertingNotificationService> notificationServiceMock) = CreateService(getContext);
			notificationServiceMock.Setup(n => n.SendAndUpdateRemindersAsync(It.Is<Appointment>(a => a.Id == appointment.Id))).CallBase();
			notificationServiceMock.Setup(n => n.SendAndUpdateSummariesAsync(It.Is<Appointment>(a => a.Id == appointment.Id))).CallBase();
			await service.RunAsync();

			// Assert
			notificationServiceMock.Verify(n => n.SendAndUpdateRemindersAsync(It.Is<Appointment>(a => a.Id == appointment.Id)), Times.Once);
			notificationServiceMock.Verify(n => n.SendAndUpdateSummariesAsync(It.Is<Appointment>(a => a.Id == appointment.Id)), Times.Once);
		}

		private static async Task<Appointment> CreateTestDataAsync(GetDatabaseContext getContext, DateTime startTime)
		{
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				var @event = new Event
				{
					Title = "Upcoming event",
					Description = "An event with upcoming appointments",
					MeetingPlace = "Somewhere",
					Organizer = john,
					ReminderTimeWindowInHours = ReminderTimeWindowInHours,
					SummaryTimeWindowInHours = SummaryTimeWindowInHours,
					EventParticipations = new List<EventParticipation>
					{
						new EventParticipation { Participant = john },
						new EventParticipation { Participant = richard }
					}
				};

				var appointment = new Appointment
				{
					Event = @event,
					AppointmentParticipations = new List<AppointmentParticipation>
					{
						new AppointmentParticipation { Participant = john },
						new AppointmentParticipation { Participant = richard }
					},
					StartTime = startTime
				};

				context.Appointments.Add(appointment);

				await context.SaveChangesAsync();

				return appointment;
			}
		}

		private static (CronSendNotificationsService service, Mock<AssertingNotificationService> notificationServiceMock) CreateService(GetDatabaseContext getContext)
		{
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);
			var service = new CronSendNotificationsService(notificationServiceMock.Object, getContext);
			return (service, notificationServiceMock);
		}

		private const int ReminderTimeWindowInHours = 2;
		private const int SummaryTimeWindowInHours = 1;
	}
}
