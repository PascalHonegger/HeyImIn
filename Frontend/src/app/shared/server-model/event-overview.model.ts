import { EventOverviewInformation } from './event-overview-information.model';

export interface EventOverview {
	yourEvents: readonly EventOverviewInformation[];
	publicEvents: readonly EventOverviewInformation[];
}
