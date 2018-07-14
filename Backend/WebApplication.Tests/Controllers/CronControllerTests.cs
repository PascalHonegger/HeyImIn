using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public class CronControllerTests : ControllerTestBase
	{
		public CronControllerTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Run_GivenFunctioningService_CallsCronServiceRunAsync()
		{
			var cronServiceMock = new Mock<ICronService>();
			cronServiceMock.Setup(c => c.DescriptiveName).Returns("Some descriptive name");

			var cronController = new CronController(new[] { cronServiceMock.Object }, DummyLogger<CronController>());
			IActionResult response = await cronController.Run();

			cronServiceMock.Verify(c => c.RunAsync(), Times.Once);
			Assert.IsType<OkResult>(response);
		}

		[Fact]
		public async Task Run_GivenServiceThrowingError_StillCallsAllCronServices()
		{
			var workingCronService1 = new Mock<ICronService>();
			workingCronService1.Setup(c => c.DescriptiveName).Returns("Working cron service #1");
			var workingCronService2 = new Mock<ICronService>();
			workingCronService2.Setup(c => c.DescriptiveName).Returns("Working cron service #2");

			const string FailingCronName = "Failing cron service";
			const string ExceptionMessage = "This cron service had some unexpected error";
			var failingCronService = new Mock<ICronService>();
			failingCronService.Setup(c => c.DescriptiveName).Returns(FailingCronName);
			failingCronService.Setup(c => c.RunAsync()).ThrowsAsync(new Exception(ExceptionMessage));

			var cronController = new CronController(new[] { workingCronService1.Object, failingCronService.Object, workingCronService2.Object }, DummyLogger<CronController>());
			IActionResult response = await cronController.Run();

			workingCronService1.Verify(c => c.RunAsync(), Times.Once);
			workingCronService2.Verify(c => c.RunAsync(), Times.Once);
			failingCronService.Verify(c => c.RunAsync(), Times.Once);
			Assert.IsType<ObjectResult>(response);
			var statusCodeResult = (ObjectResult)response;
			Assert.Equal(500, statusCodeResult.StatusCode);
			var errors = statusCodeResult.Value as List<(string, string)>;
			Assert.NotNull(errors);
			Assert.Single(errors);
			Assert.Equal((FailingCronName, ExceptionMessage), errors[0]);
		}
	}
}
