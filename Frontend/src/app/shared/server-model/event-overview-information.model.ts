import { ViewEventInformation } from './view-event-information.model';
import { AppointmentDetails } from './appointment-details.model';

export interface EventOverviewInformation {
	eventId: number;
	viewEventInformation: ViewEventInformation;
	latestAppointmentDetails: AppointmentDetails;
}
