using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests
	{
		[Fact]
		public async Task SetNewPassword_GivenWrongCurrentPassword_DoesNotChangePassword()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			User john;
			const string WrongCurrentPassword = "Wrong password";

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				john = context.Users.Add(ContextUtilities.CreateJohnDoe()).Entity;

				await context.SaveChangesAsync();
			}

			// Act
			(UserController controller, _, Mock<IPasswordService> passwordServiceMock, _) = CreateController(getContext, john.Id);
			passwordServiceMock.Setup(p => p.VerifyPassword(WrongCurrentPassword, john.PasswordHash)).Returns(false);

			IActionResult response = await controller.SetNewPassword(new SetPasswordDto { CurrentPassword = WrongCurrentPassword, NewPassword = "New Password"});

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.CurrentPasswordWrong, objectResult.Value);
		}

		[Fact]
		public async Task SetNewPassword_GivenCorrectCurrentPassword_ChangesPassword()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			User john;
			const string CorrectCurrentPassword = "Correct password";
			const string NewPassword = "New Password";
			const string NewPasswordHash = "Néw P@ssw0rd";

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				john = context.Users.Add(ContextUtilities.CreateJohnDoe()).Entity;

				await context.SaveChangesAsync();
			}

			// Act
			(UserController controller, _, Mock<IPasswordService> passwordServiceMock, _) = CreateController(getContext, john.Id);
			passwordServiceMock.Setup(p => p.VerifyPassword(CorrectCurrentPassword, john.PasswordHash)).Returns(true);
			passwordServiceMock.Setup(p => p.HashPassword(NewPassword)).Returns(NewPasswordHash);

			IActionResult response = await controller.SetNewPassword(new SetPasswordDto { CurrentPassword = CorrectCurrentPassword, NewPassword = NewPassword });

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				User loadedUser = context.Users.Single();
				Assert.Equal(NewPasswordHash, loadedUser.PasswordHash);
			}
		}
	}
}
