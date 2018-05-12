using Microsoft.Extensions.Logging;
using Moq;

namespace HeyImIn.Shared.Tests
{
	public abstract class TestBase
	{
		protected static ILogger<T> DummyLogger<T>()
		{
			return new Mock<ILogger<T>>(MockBehavior.Loose).Object;
		}

		protected ILoggerFactory DummyLoggerFactory()
		{
			var mock = new Mock<ILoggerFactory>(MockBehavior.Strict);
			mock.Setup(lf => lf.CreateLogger(LogHelpers.AuditLoggerName)).Returns(DummyLogger<object>());
			return mock.Object;
		}
	}
}
