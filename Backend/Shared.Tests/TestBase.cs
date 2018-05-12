using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.Shared.Tests
{
	public abstract class TestBase
	{
		protected ITestOutputHelper Output { get; }

		public TestBase(ITestOutputHelper output)
		{
			Output = output;
		}

		protected ILogger<T> DummyLogger<T>()
		{
			return new XUnitLogger<T>(Output);
		}

		protected ILoggerFactory DummyLoggerFactory()
		{
			var mock = new Mock<ILoggerFactory>(MockBehavior.Strict);
			mock.Setup(lf => lf.CreateLogger(LogHelpers.AuditLoggerName)).Returns(DummyLogger<object>());
			return mock.Object;
		}
	}
}
