import { EventChatMessage } from './event-chat-message.model';

export interface EventChatMessages {
	messages: EventChatMessage[];
	possiblyMoreMessages: boolean;
}
