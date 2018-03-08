using System.Web;
using System.Web.Http;

namespace HeyImIn.WebApplication
{
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			// TODO Configure log4net

			// TODO Log Started

			GlobalConfiguration.Configure(WebApiConfig.Register);
		}

		protected void Application_End()
		{
			// TODO Log Stopped
		}
	}
}