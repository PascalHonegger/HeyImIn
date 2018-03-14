import { GeneralEventInfo } from './general-event-info.model';

export interface ViewEventInformation extends GeneralEventInfo {
	organizerId: number;
	organizerName: string;
	totalParticipants: number;
	currentUserDoesParticipate: boolean;
}
