using System;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ResetPasswordControllerTests
	{
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
	}
}
