using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes
{
	public class EventOverview
	{
		public EventOverview(List<EventOverviewInformation> yourEvents, List<EventOverviewInformation> publicEvents)
		{
			YourEvents = yourEvents;
			PublicEvents = publicEvents;
		}

		public List<EventOverviewInformation> YourEvents { get; }

		public List<EventOverviewInformation> PublicEvents { get; }
	}
}
