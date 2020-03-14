using System;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests
	{
		[Fact]
		public async Task Register_GivenCorrectData_UserRegistered()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Arrange
			var data = new RegisterDto { Email = "new@user.com", FullName = "New User", Password = "Password" };
			var newSessionGuid = Guid.NewGuid();
			const string PasswordHash = "P@ssw0rd";

			// Act
			(UserController controller, Mock<ISessionService> sessionServiceMock, Mock<IPasswordService> passwordServiceMock, _) = CreateController(getContext, null);
			sessionServiceMock.Setup(s => s.CreateSessionAsync(It.IsAny<int>(), true)).ReturnsAsync(newSessionGuid);
			passwordServiceMock.Setup(s => s.HashPassword(data.Password)).Returns(PasswordHash);
			IActionResult response = await controller.Register(data);

			// Assert
			sessionServiceMock.Verify(s => s.CreateSessionAsync(It.IsAny<int>(), true), Times.Once);
			passwordServiceMock.Verify(s => s.HashPassword(data.Password), Times.Once);

			Assert.IsType<OkObjectResult>(response);
			var objectResult = (OkObjectResult)response;
			var session = objectResult.Value as FrontendSession;
			using (IDatabaseContext context = getContext())
			{
				Assert.Single(context.Users);
				User loadedUser = context.Users.First();
				Assert.Equal(data.FullName, loadedUser.FullName);
				Assert.Equal(data.Email, loadedUser.Email);
				Assert.Equal(PasswordHash, loadedUser.PasswordHash);

				Assert.NotNull(session);
				Assert.Equal(loadedUser.Id, session.UserId);
				Assert.Equal(loadedUser.Email, session.Email);
				Assert.Equal(loadedUser.FullName, session.FullName);
				Assert.Equal(newSessionGuid, session.Token);
			}
		}

		[Fact]
		public async Task Register_GivenExistingEmail_UserNotRegistered()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			// Arrange
			var data = new RegisterDto { Email = "new@user.com", FullName = "New User", Password = "Password" };
			using (IDatabaseContext context = getContext())
			{
				context.Users.Add(new User
				{
					Email = data.Email,
					FullName = "Some other name",
					PasswordHash = "Some hashed password"
				});

				await context.SaveChangesAsync();
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, null);
			IActionResult response = await controller.Register(data);

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.EmailAlreadyInUse, objectResult.Value);
		}
	}
}
