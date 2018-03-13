using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using HeyImIn.WebApplication.Services;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	public class CronController : ApiController
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
		public void Run()
		{
			_log.DebugFormat("{0}(): Running Cron jobs", nameof(Run));
			
			Parallel.ForEach(_cronRunners, async runner => await runner.RunAsync());
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}
