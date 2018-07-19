import { AppointmentParticipationAnswer } from './appointment-participation-answer.model';

export interface AppointmentParticipationInformation {
	participantId: number;
	response?: AppointmentParticipationAnswer;
}
