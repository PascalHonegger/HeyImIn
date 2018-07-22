import { GeneralEventInformation } from './general-event-information.model';
import { AppointmentDetails } from './appointment-details.model';
import { UserInformation } from './user-information.model';

export interface EditEventDetails {
	information: GeneralEventInformation;
	upcomingAppointments: ReadonlyArray<AppointmentDetails>;
	participants: ReadonlyArray<UserInformation>;
}
