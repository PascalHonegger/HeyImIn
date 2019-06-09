import { EventChatMessage } from './event-chat-message.model';
import { UserInformation } from './user-information.model';

export interface EventChatMessages {
	messages: readonly EventChatMessage[];
	possiblyMoreMessages: boolean;
	authorInformations: readonly UserInformation[];
}
