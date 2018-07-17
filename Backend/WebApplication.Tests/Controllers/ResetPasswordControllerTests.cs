using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public class ResetPasswordControllerTests : ControllerTestBase
	{
		public ResetPasswordControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task RequestPasswordReset_GivenExistingUser_SendsResetToken()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnDoeId;
			string johnDoeEmail;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());

				await context.SaveChangesAsync();

				johnDoeId = john.Entity.Id;
				johnDoeEmail = john.Entity.Email;
			}

			// Act
			(ResetPasswordController controller, _, Mock<AssertingNotificationService> notificationServiceMock) = CreateController(getContext, null);
			Expression<Func<AssertingNotificationService, Task>> sendPasswordResetExpression = n => n.SendPasswordResetTokenAsync(It.IsAny<Guid>(), It.Is<User>(u => u.Id == johnDoeId));
			notificationServiceMock.Setup(sendPasswordResetExpression).CallBase();

			IActionResult response = await controller.RequestPasswordReset(new RequestPasswordResetDto { Email = johnDoeEmail });

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				notificationServiceMock.Verify(sendPasswordResetExpression, Times.Once);
				PasswordReset passwordReset = await context.PasswordResets.SingleOrDefaultAsync();
				Assert.NotNull(passwordReset);
				Assert.False(passwordReset.Used);
				Assert.Equal(johnDoeId, passwordReset.UserId);
			}
		}

		[Fact]
		public async Task RequestPasswordReset_GivenNonExistingUser_RequestRejected()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Act
			(ResetPasswordController controller, _, _) = CreateController(getContext, null);

			IActionResult response = await controller.RequestPasswordReset(new RequestPasswordResetDto { Email = "random@user.com" });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var badRequestResponse = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.NoProfileWithEmailFound, badRequestResponse.Value);
			using (IDatabaseContext context = getContext())
			{
				Assert.Empty(context.PasswordResets);
			}
		}

		[Fact]
		public async Task ResetPassword_GivenValidPasswordReset_NewPasswordIsSet()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			Guid resetToken;
			const string NewPassword = "Password";
			const string NewPasswordHash = "P@ssw0rd";

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				var reset = new PasswordReset { Requested = DateTime.UtcNow, User = john };
				context.PasswordResets.Add(reset);

				await context.SaveChangesAsync();

				resetToken = reset.Token;
			}

			// Act
			(ResetPasswordController controller, Mock<IPasswordService> passwordServiceMock, _) = CreateController(getContext, null);
			passwordServiceMock.Setup(p => p.HashPassword(NewPassword)).Returns(NewPasswordHash);

			IActionResult response = await controller.ResetPassword(new ResetPasswordDto { NewPassword = NewPassword, PasswordResetToken = resetToken });

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				passwordServiceMock.Verify(p => p.HashPassword(NewPassword), Times.Once);
				PasswordReset passwordReset = await context.PasswordResets.Include(p => p.User).SingleOrDefaultAsync();
				Assert.NotNull(passwordReset);
				Assert.True(passwordReset.Used);
				Assert.Equal(NewPasswordHash, passwordReset.User.PasswordHash);
			}
		}

		[Fact]
		public async Task ResetPassword_GivenInvalidPasswordReset_RequestRejected()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Act
			(ResetPasswordController controller, _, _) = CreateController(getContext, null);

			IActionResult response = await controller.ResetPassword(new ResetPasswordDto { NewPassword = "Doesn't matter", PasswordResetToken = Guid.NewGuid() });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var badRequestResponse = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.ResetCodeInvalid, badRequestResponse.Value);
		}

		[Fact]
		public async Task ResetPassword_GivenUsedPasswordReset_RequestRejected()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			Guid resetToken;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				var reset = new PasswordReset { Requested = DateTime.UtcNow, User = john, Used = true };
				context.PasswordResets.Add(reset);

				await context.SaveChangesAsync();

				resetToken = reset.Token;
			}

			// Act
			(ResetPasswordController controller, _, _) = CreateController(getContext, null);

			IActionResult response = await controller.ResetPassword(new ResetPasswordDto { NewPassword = "Doesn't matter", PasswordResetToken = resetToken });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var badRequestResponse = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.ResetCodeAlreadyUsedOrExpired, badRequestResponse.Value);
		}

		[Fact]
		public async Task ResetPassword_GivenExpiredPasswordReset_RequestRejected()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			Guid resetToken;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				var reset = new PasswordReset { Requested = DateTime.UtcNow - _configuration.TimeSpans.PasswordResetTimeout, User = john, Used = true };
				context.PasswordResets.Add(reset);

				await context.SaveChangesAsync();

				resetToken = reset.Token;
			}

			// Act
			(ResetPasswordController controller, _, _) = CreateController(getContext, null);

			IActionResult response = await controller.ResetPassword(new ResetPasswordDto { NewPassword = "Doesn't matter", PasswordResetToken = resetToken });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var badRequestResponse = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.ResetCodeAlreadyUsedOrExpired, badRequestResponse.Value);
		}

		private (ResetPasswordController participateEventController, Mock<IPasswordService> passwordServiceServiceMock, Mock<AssertingNotificationService> notificationMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var passwordServiceServiceMock = new Mock<IPasswordService>(MockBehavior.Strict);
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);

			var controller = new ResetPasswordController(passwordServiceServiceMock.Object, notificationServiceMock.Object, _configuration, getContext, DummyLogger<ResetPasswordController>())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, passwordServiceServiceMock, notificationServiceMock);
		}

		private readonly HeyImInConfiguration _configuration = new HeyImInConfiguration();
	}
}
