using System;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.WebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests
	{
		[Fact]
		public async Task DeleteAccount_GivenSimpleUser_UserDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnDoeId);
			IActionResult response = await controller.DeleteAccount();

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
			}
		}

		[Fact]
		public async Task DeleteAccount_GivenUserWithSimpleRelations_UserAndRelationsDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				context.PasswordResets.Add(new PasswordReset { Requested = DateTime.UtcNow, User = john });
				context.Sessions.Add(new Session { Created = DateTime.UtcNow, User = john });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnDoeId);
			IActionResult response = await controller.DeleteAccount();

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
				Assert.Empty(context.Sessions);
				Assert.Empty(context.PasswordResets);
			}
		}

		[Fact]
		public async Task DeleteAccount_GivenUserWithEvents_UserAndEventsDeleted()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			int event1Id;
			int event2Id;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();

				Event event1 = DummyEvent(john);
				Event event2 = DummyEvent(john, true);
				context.EventParticipations.Add(new EventParticipation { Event = event1, Participant = john });
				context.EventParticipations.Add(new EventParticipation { Event = event2, Participant = john });

				context.EventInvitations.Add(new EventInvitation { Event = event1, Requested = DateTime.UtcNow });
				context.EventInvitations.Add(new EventInvitation { Event = event2, Requested = DateTime.UtcNow });

				await context.SaveChangesAsync();

				johnDoeId = john.Id;
				event1Id = event1.Id;
				event2Id = event2.Id;
			}

			// Act
			(UserController controller, _, _, Mock<INotificationService> notificationMock) = CreateController(getContext, johnDoeId);

			EventNotificationInformation notificationInformation1 = null;
			notificationMock
				.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == event1Id)))
				.Callback<EventNotificationInformation>(e => notificationInformation1 = e)
				.Returns(Task.CompletedTask);

			EventNotificationInformation notificationInformation2 = null;
			notificationMock
				.Setup(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == event2Id)))
				.Callback<EventNotificationInformation>(e => notificationInformation2 = e)
				.Returns(Task.CompletedTask);

			IActionResult response = await controller.DeleteAccount();

			// Assert
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == event1Id)), Times.Once);
			notificationMock.Verify(n => n.NotifyEventDeletedAsync(It.Is<EventNotificationInformation>(e => e.Id == event2Id)), Times.Once);

			AssertNotificationInformation(notificationInformation1);
			AssertNotificationInformation(notificationInformation2);

			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.Users);
				Assert.Empty(context.Events);
				Assert.Empty(context.EventParticipations);
			}

			void AssertNotificationInformation(EventNotificationInformation notificationInformation)
			{
				Assert.NotNull(notificationInformation);
				Assert.NotNull(notificationInformation.Title);
				Assert.NotNull(notificationInformation.Participations);
				foreach (UserNotificationInformation participation in notificationInformation.Participations)
				{
					Assert.NotNull(participation);
					Assert.NotNull(participation.Email);
					Assert.NotNull(participation.FullName);
				}
			}
		}
	}
}
