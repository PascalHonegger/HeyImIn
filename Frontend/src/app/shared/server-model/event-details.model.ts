import { ViewEventInformation } from './view-event-information.model';
import { AppointmentDetails } from './appointment-details.model';
import { NotificationConfiguration } from './notification-configuration.model';

export interface EventDetails {
	information: ViewEventInformation;
	upcomingAppointments: ReadonlyArray<AppointmentDetails>;
	notificationConfigurationResponse: NotificationConfiguration;
}
