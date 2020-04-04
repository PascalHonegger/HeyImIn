import type { EventChatMessage } from './event-chat-message.model';
import type { UserInformation } from './user-information.model';

export interface EventChatMessages {
	messages: readonly EventChatMessage[];
	possiblyMoreMessages: boolean;
	authorInformations: readonly UserInformation[];
}
