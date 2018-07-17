using System.Threading;
using System.Threading.Tasks;

namespace HeyImIn.WebApplication.Services
{
	/// <summary>
	///     A service for repeating scheduled events
	/// </summary>
	public interface ICronService
	{
		/// <summary>
		///     Runs the cron service
		/// </summary>
		Task RunAsync(CancellationToken token);

		/// <summary>
		///     A name describing this service
		///     E.g. CleanupLogCron, SendNotificationsCron, ...
		/// </summary>
		string DescriptiveName { get; }
	}
}
