using System.Threading.Tasks;

namespace HeyImIn.WebApplication.Services
{
	public interface ICronService
	{
		Task RunAsync();
	}
}