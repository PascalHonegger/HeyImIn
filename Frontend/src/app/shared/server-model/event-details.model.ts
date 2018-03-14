import { GeneralEventInfo } from './general-event-info.model';
import { AppointmentDetails } from './event-edit-details.model';
import { ViewEventInformation } from './view-event-information.model';
import { NotificationConfiguration } from './notification-configuration.model';

export interface EventDetails {
	information: ViewEventInformation;
	upcomingAppointments: AppointmentDetails[];
	notificationConfigurationResponse: NotificationConfiguration;
}
