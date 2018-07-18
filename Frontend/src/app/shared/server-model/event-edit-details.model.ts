import { GeneralEventInfo } from './general-event-info.model';
import { AppointmentParticipationAnswer } from './appointment-participation-answer.model';

export interface EditEventDetails {
	information: GeneralEventInfo;
	upcomingAppointments: AppointmentDetails[];
	participants: EventParticipantInformation[];
}

export interface AppointmentDetails {
	appointmentInformation: AppointmentInformation;
	participations: AppointmentParticipationInformation[];
}

export interface AppointmentInformation {
	appointmentId: number;
	startTime: Date | string;
	currentResponse?: AppointmentParticipationAnswer;
	acceptedParticipants: number;
	declinedParticipants: number;
	notAnsweredParticipants: number;
}

export interface AppointmentParticipationInformation {
	participantName: string;
	participantId: number;
	response?: AppointmentParticipationAnswer;
}

export interface EventParticipantInformation {
	participantName: string;
	participantId: number;
	participantEmail: string;
}
