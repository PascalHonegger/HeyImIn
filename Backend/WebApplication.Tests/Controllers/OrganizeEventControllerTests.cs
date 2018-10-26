using HeyImIn.Database.Context;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Services.Impl;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class OrganizeEventControllerTests : ControllerTestBase
	{
		public OrganizeEventControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		private (OrganizeEventController controller, Mock<AssertingNotificationService> notificationServiceMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);

			var controller = new OrganizeEventController(notificationServiceMock.Object, new DeleteService(), getContext, DummyLogger<OrganizeEventController>(), DummyLoggerFactory())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, notificationServiceMock);
		}
	}
}
