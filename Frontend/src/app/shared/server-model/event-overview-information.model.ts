import type { ViewEventInformation } from './view-event-information.model';
import type { AppointmentDetails } from './appointment-details.model';

export interface EventOverviewInformation {
	eventId: number;
	viewEventInformation: ViewEventInformation;
	latestAppointmentDetails: AppointmentDetails;
}
