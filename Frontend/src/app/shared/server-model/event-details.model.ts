import type { ViewEventInformation } from './view-event-information.model';
import type { AppointmentDetails } from './appointment-details.model';
import type { NotificationConfiguration } from './notification-configuration.model';

export interface EventDetails {
	information: ViewEventInformation;
	upcomingAppointments: readonly AppointmentDetails[];
	notificationConfigurationResponse: NotificationConfiguration;
}
