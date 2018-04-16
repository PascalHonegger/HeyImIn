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
	public class CronController : Controller
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
		[HttpPost]
		public async Task<IHttpActionResult> Run()
		{
			_log.DebugFormat("{0}(): Running Cron jobs", nameof(Run));

			var cronStopwatch = new Stopwatch();
			var hadError = false;

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

					hadError = true;
				}
				finally
				{
					cronStopwatch.Stop();
					_log.DebugFormat("{0}(): Finished running '{1}', duration = {2:g}", nameof(Run), cronService.DescriptiveName, cronStopwatch.Elapsed);
				}
			}

			if (hadError)
			{
				return InternalServerError();
			}

			return Ok();
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
