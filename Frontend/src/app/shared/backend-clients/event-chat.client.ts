import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import { EventChatMessages } from '../server-model/event-chat-messages.model';
import { EventChatMessage } from '../server-model/event-chat-message.model';

@Injectable({ providedIn: 'root' })
export class EventChatClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('EventChat');
	}

	public getChatMessages(eventId: number, earliestLoadedMessage: EventChatMessage | null = null) {
/*		const queryParams: { [param: string]: string } = {
			eventId: eventId.toString()
		};

		if (earliestLoadedMessage != null) {
			if (typeof(earliestLoadedMessage.sentDate) !== 'string') {
				console.warn('Got a date object instead of a string, this could lead to weird precision issues');
				queryParams.earliestLoadedMessageSentDate = earliestLoadedMessage.sentDate.toJSON();
			} else {
				queryParams.earliestLoadedMessageSentDate = earliestLoadedMessage.sentDate;
			}
		}*/

		return this.httpClient.post<EventChatMessages>(this.baseUrl + '/GetChatMessages',
			{ eventId, earliestLoadedMessageSentDate: earliestLoadedMessage && earliestLoadedMessage.sentDate });
	}

	public sendChatMessage(eventId: number, content: string) {
		return this.httpClient.post<EventChatMessage>(this.baseUrl + '/SendChatMessage', { eventId, content });
	}
}
