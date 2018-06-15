using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HeyImIn.Shared.Tests
{
	public class XUnitLoggerFactory : ILoggerFactory
	{
		public XUnitLoggerFactory(ITestOutputHelper output)
		{
			_output = output;
		}

		public void Dispose()
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new XUnitLogger<object>(_output);
		}

		public void AddProvider(ILoggerProvider provider)
		{
			throw new NotImplementedException();
		}

		private readonly ITestOutputHelper _output;
	}
}
