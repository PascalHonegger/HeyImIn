using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Controllers;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ResetPasswordControllerTests : ControllerTestBase
	{
		public ResetPasswordControllerTests(ITestOutputHelper output) : base(output)
		{
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
