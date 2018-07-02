using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Services.Impl;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class UserControllerTests : ControllerTestBase
	{
		public UserControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		private (UserController controller, Mock<ISessionService> sessionServiceMock, Mock<IPasswordService> passwordServiceServiceMock, Mock<AssertingNotificationService> notificationServiceMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var passwordServiceServiceMock = new Mock<IPasswordService>(MockBehavior.Strict);
			var sessionServiceMock = new Mock<ISessionService>(MockBehavior.Strict);
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);

			var controller = new UserController(passwordServiceServiceMock.Object, sessionServiceMock.Object, notificationServiceMock.Object, new DeleteService(), getContext, DummyLogger<UserController>(), DummyLoggerFactory())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, sessionServiceMock, passwordServiceServiceMock, notificationServiceMock);
		}
	}
}
