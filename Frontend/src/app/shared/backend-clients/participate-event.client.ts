import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { EventOverview } from '../server-model/event-overview.model';
import type { EventDetails } from '../server-model/event-details.model';
import type { NotificationConfiguration } from '../server-model/notification-configuration.model';

@Injectable({ providedIn: 'root' })
export class ParticipateEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('ParticipateEvent');
	}

	public getOverview() {
		return this.httpClient.get<EventOverview>(this.baseUrl + '/GetOverview');
	}

	public getDetails(eventId: number) {
		return this.httpClient.get<EventDetails>(this.baseUrl + '/GetDetails', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}

	public joinEvent(eventId: number) {
		return this.httpClient.post<void>(this.baseUrl + '/JoinEvent', { eventId });
	}

	public removeFromEvent(eventId: number, userId: number) {
		return this.httpClient.post<void>(this.baseUrl + '/RemoveFromEvent', { eventId, userId });
	}

	public configureNotifications(eventId: number, notifications: NotificationConfiguration) {
		const combinedDto = Object.assign({ eventId }, notifications);
		return this.httpClient.post<void>(this.baseUrl + '/ConfigureNotifications', combinedDto);
	}
}
