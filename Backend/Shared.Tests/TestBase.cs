using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace HeyImIn.Shared.Tests
{
	public abstract class TestBase
	{
		private readonly ITestOutputHelper _output;

		protected TestBase(ITestOutputHelper output)
		{
			_output = output;
		}

		protected ILogger<T> DummyLogger<T>()
		{
			return new XUnitLogger<T>(_output);
		}

		protected ILoggerFactory DummyLoggerFactory()
		{
			var mock = new Mock<ILoggerFactory>(MockBehavior.Strict);
			mock.Setup(lf => lf.CreateLogger(LogHelpers.AuditLoggerName)).Returns(DummyLogger<object>());
			return mock.Object;
		}
	}
}
