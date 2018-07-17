using HeyImIn.Database.Context;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Controllers;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class EventChatControllerTests : ControllerTestBase
	{
		public EventChatControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		private (EventChatController controller, Mock<AssertingNotificationService> notificationServiceMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);

			var controller = new EventChatController(notificationServiceMock.Object, _configuration, getContext, DummyLogger<EventChatController>(), DummyLoggerFactory())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, notificationServiceMock);
		}

		private readonly HeyImInConfiguration _configuration = new HeyImInConfiguration();
	}
}
