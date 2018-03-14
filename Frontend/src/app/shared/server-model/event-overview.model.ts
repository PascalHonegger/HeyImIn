import { GeneralEventInfo } from './general-event-info.model';
import { AppointmentInformation } from './event-edit-details.model';
import { ViewEventInformation } from './view-event-information.model';

export interface EventOverview {
	yourEvents: EventOverviewInformation[];
	publicEvents: EventOverviewInformation[];
}

export interface EventOverviewInformation {
	eventId: number;
	viewEventInformation: ViewEventInformation;
	latestAppointmentInformation?: AppointmentInformation;
}
