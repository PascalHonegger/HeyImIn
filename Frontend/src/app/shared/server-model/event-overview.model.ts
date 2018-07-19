import { EventOverviewInformation } from './event-overview-information.model';

export interface EventOverview {
	yourEvents: ReadonlyArray<EventOverviewInformation>;
	publicEvents: ReadonlyArray<EventOverviewInformation>;
}
