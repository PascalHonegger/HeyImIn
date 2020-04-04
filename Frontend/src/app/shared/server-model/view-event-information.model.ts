import type { GeneralEventInformation } from './general-event-information.model';
import type { UserInformation } from './user-information.model';

export interface ViewEventInformation extends GeneralEventInformation {
	organizer: UserInformation;
	participants: readonly UserInformation[];
}
