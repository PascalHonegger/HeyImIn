using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ResetPasswordControllerTests
	{
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
	}
}
