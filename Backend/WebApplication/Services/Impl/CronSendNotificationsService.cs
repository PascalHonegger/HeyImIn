using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace HeyImIn.WebApplication.Services.Impl
{
	public class CronSendNotificationsService : ICronService
	{
		public async Task RunAsync()
		{
			// TODO Remove
			await Task.Delay(100);
			_log.WarnFormat("{0}(): Sending of notifications is not yet implemented", nameof(RunAsync));
		}

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}