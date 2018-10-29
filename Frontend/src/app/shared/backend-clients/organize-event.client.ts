import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import { GeneralEventInformation } from '../server-model/general-event-information.model';
import { EditEventDetails } from '../server-model/edit-event-details.model';

@Injectable({ providedIn: 'root' })
export class OrganizeEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('OrganizeEvent');
	}

	public deleteEvent(eventId: number) {
		return this.httpClient.delete<void>(this.baseUrl + '/DeleteEvent', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}

	public updateEventInfo(eventId: number, eventInfo: GeneralEventInformation) {
		const combinedDto = Object.assign({ eventId }, eventInfo);
		return this.httpClient.post<void>(this.baseUrl + '/UpdateEventInfo', combinedDto);
	}

	public changeOrganizer(eventId: number, newOrganizerId: number) {
		return this.httpClient.post<void>(this.baseUrl + '/ChangeOrganizer', { eventId, newOrganizerId });
	}

	public createEvent(eventInfo: GeneralEventInformation) {
		return this.httpClient.post<void>(this.baseUrl + '/CreateEvent', eventInfo);
	}

	public getEditDetails(eventId: number) {
		return this.httpClient.get<EditEventDetails>(this.baseUrl + '/GetEditDetails', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}
}
