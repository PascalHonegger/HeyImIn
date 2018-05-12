using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HeyImIn.WebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/Cron")]
	public class CronController : ControllerBase
	{
		private readonly IEnumerable<ICronService> _cronRunners;

		public CronController(IEnumerable<ICronService> cronRunners, ILogger<CronController> logger)
		{
			_cronRunners = cronRunners;
			_logger = logger;
		}

		/// <summary>
		///     Runs all cron-jobs
		///     Catches and logs exceptions thrown by the <see cref="ICronService.RunAsync"/> method
		/// </summary>
		[HttpPost(nameof(Run))]
		[ProducesResponseType(typeof(void), 200)]
		[ProducesResponseType(typeof(List<(string, string)>), 500)]
		public async Task<IActionResult> Run()
		{
			_logger.LogDebug("{0}(): Running Cron jobs", nameof(Run));

			var cronStopwatch = new Stopwatch();
			var errors = new List<(string jobName, string errorMessage)>();

			foreach (ICronService cronService in _cronRunners)
			{
				_logger.LogDebug("{0}(): Start running '{1}'", nameof(Run), cronService.DescriptiveName);
				cronStopwatch.Restart();

				try
				{
					await cronService.RunAsync();
				}
				catch (Exception e)
				{
					_logger.LogError("{0}(): Error while running '{1}', error={2}", nameof(Run), cronService.DescriptiveName, e);

					errors.Add((cronService.DescriptiveName, e.Message));
				}
				finally
				{
					cronStopwatch.Stop();
					_logger.LogDebug("{0}(): Finished running '{1}', duration = {2:g}", nameof(Run), cronService.DescriptiveName, cronStopwatch.Elapsed);
				}
			}

			if (errors.Count != 0)
			{
				return StatusCode(500, errors);
			}

			return Ok();
		}

		private readonly ILogger<CronController> _logger;
	}
}
