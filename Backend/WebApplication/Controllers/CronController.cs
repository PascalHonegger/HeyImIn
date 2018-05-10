using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.WebApplication.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/Cron")]
	public class CronController : ControllerBase
	{
		private readonly IEnumerable<ICronService> _cronRunners;

		public CronController(IEnumerable<ICronService> cronRunners)
		{
			_cronRunners = cronRunners;
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
			_log.DebugFormat("{0}(): Running Cron jobs", nameof(Run));

			var cronStopwatch = new Stopwatch();
			var errors = new List<(string jobName, string errorMessage)>();

			foreach (ICronService cronService in _cronRunners)
			{
				_log.DebugFormat("{0}(): Start running '{1}'", nameof(Run), cronService.DescriptiveName);
				cronStopwatch.Restart();

				try
				{
					await cronService.RunAsync();
				}
				catch (Exception e)
				{
					_log.ErrorFormat("{0}(): Error while running '{1}', error={2}", nameof(Run), cronService.DescriptiveName, e);

					errors.Add((cronService.DescriptiveName, e.Message));
				}
				finally
				{
					cronStopwatch.Stop();
					_log.DebugFormat("{0}(): Finished running '{1}', duration = {2:g}", nameof(Run), cronService.DescriptiveName, cronStopwatch.Elapsed);
				}
			}

			if (errors.Count != 0)
			{
				return StatusCode(500, errors);
			}

			return Ok();
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
