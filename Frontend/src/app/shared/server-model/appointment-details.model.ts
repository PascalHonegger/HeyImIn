import { AppointmentParticipationInformation } from './appointment-participation-information.model';

export interface AppointmentDetails {
	appointmentId: number;
	startTime: Date;
	participations: readonly AppointmentParticipationInformation[];
}
