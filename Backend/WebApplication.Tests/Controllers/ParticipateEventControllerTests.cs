using HeyImIn.Database.Context;
using HeyImIn.MailNotifier.Tests;
using HeyImIn.Shared;
using HeyImIn.WebApplication.Controllers;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests : ControllerTestBase
	{
		public ParticipateEventControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		private (ParticipateEventController participateEventController, Mock<AssertingNotificationService> notificationMock) CreateController(GetDatabaseContext getContext, int? currentUserId)
		{
			var notificationServiceMock = new Mock<AssertingNotificationService>(MockBehavior.Strict);

			var controller = new ParticipateEventController(notificationServiceMock.Object, new HeyImInConfiguration { MaxAmountOfAppointmentsPerDetailPage = MaxAmountOfAppointments }, getContext, DummyLogger<ParticipateEventController>(), DummyLoggerFactory())
			{
				ControllerContext = CurrentUserContext(currentUserId)
			};

			return (controller, notificationServiceMock);
		}

		private const int MaxAmountOfAppointments = 10;
	}
}
