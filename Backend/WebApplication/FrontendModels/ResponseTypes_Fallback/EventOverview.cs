using System.Collections.Generic;

namespace HeyImIn.WebApplication.FrontendModels.ResponseTypes_Fallback
{
	public class EventOverview
	{
		public EventOverview(List<EventOverviewInformation> yourEvents, List<EventOverviewInformation> publicEvents)
		{
			YourEvents = yourEvents;
			PublicEvents = publicEvents;
		}

		/// <summary>
		///     Events you participate or organize
		/// </summary>
		public List<EventOverviewInformation> YourEvents { get; }

		/// <summary>
		///     Events you could participate without an invitation
		/// </summary>
		public List<EventOverviewInformation> PublicEvents { get; }
	}
}
