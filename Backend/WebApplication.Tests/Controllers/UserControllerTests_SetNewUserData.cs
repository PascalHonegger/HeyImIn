using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests
	{
		[Fact]
		public async Task SetNewUserData_GivenAlreadyUsedEmail_ErrorReturned()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnId;
			const string NewName = "New Fancy Name";
			string usedEmail;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = context.Users.Add(ContextUtilities.CreateJohnDoe()).Entity;

				User richard = context.Users.Add(ContextUtilities.CreateRichardRoe()).Entity;
				usedEmail = richard.Email;

				await context.SaveChangesAsync();

				johnId = john.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnId);

			IActionResult response = await controller.SetNewUserData(new SetUserDataDto { NewEmail = usedEmail, NewFullName = NewName });

			// Assert
			Assert.IsType<BadRequestObjectResult>(response);
			var objectResult = (BadRequestObjectResult)response;
			Assert.Equal(RequestStringMessages.EmailAlreadyInUse, objectResult.Value);
		}

		[Fact]
		public async Task SetNewUserData_GivenValidData_UserUpdated()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int johnId;
			const string NewName = "New Fancy Name";
			const string NewMail = "new@mail.com";

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = context.Users.Add(ContextUtilities.CreateJohnDoe()).Entity;

				await context.SaveChangesAsync();

				johnId = john.Id;
			}

			// Act
			(UserController controller, _, _, _) = CreateController(getContext, johnId);

			IActionResult response = await controller.SetNewUserData(new SetUserDataDto { NewEmail = NewMail, NewFullName = NewName });

			// Assert
			Assert.IsType<OkResult>(response);
			using (IDatabaseContext context = getContext())
			{
				User loadedUser = context.Users.Single();
				Assert.Equal(NewName, loadedUser.FullName);
				Assert.Equal(NewMail, loadedUser.Email);
			}
		}
	}
}
