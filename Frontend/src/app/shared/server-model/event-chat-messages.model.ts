import { EventChatMessage } from './event-chat-message.model';
import { UserInformation } from './user-information.model';

export interface EventChatMessages {
	messages: ReadonlyArray<EventChatMessage>;
	possiblyMoreMessages: boolean;
	authorInformations: ReadonlyArray<UserInformation>;
}
