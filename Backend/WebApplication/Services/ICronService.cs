using System.Threading.Tasks;

namespace HeyImIn.WebApplication.Services
{
	public interface ICronService
	{
		/// <summary>
		///     Runs the cron service
		/// </summary>
		Task RunAsync();
	}
}
