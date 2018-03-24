using System;
using System.Collections.Generic;
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
		///     Default / fallback route which redirects to index.html
		/// </summary>
		[HttpPost]
		public async Task Run()
		{
			_log.DebugFormat("{0}(): Running Cron jobs", nameof(Run));

			try
			{
				foreach (ICronService cronService in _cronRunners)
				{
					await cronService.RunAsync();
				}
			}
			catch (Exception e)
			{
				_log.ErrorFormat("{0}(): Failed to execute cron job, error={1}", nameof(Run), e);
				
				throw;
			}
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
