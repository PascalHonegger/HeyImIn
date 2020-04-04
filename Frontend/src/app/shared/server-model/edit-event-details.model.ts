import type { GeneralEventInformation } from './general-event-information.model';
import type { AppointmentDetails } from './appointment-details.model';
import type { UserInformation } from './user-information.model';

export interface EditEventDetails {
	information: GeneralEventInformation;
	upcomingAppointments: readonly AppointmentDetails[];
	participants: readonly UserInformation[];
}
