import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { GeneralEventInformation } from '../server-model/general-event-information.model';
import type { EditEventDetails } from '../server-model/edit-event-details.model';
import type { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class OrganizeEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('OrganizeEvent');
	}

	public deleteEvent(eventId: number): Observable<void> {
		return this.httpClient.delete<void>(this.baseUrl + '/DeleteEvent', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}

	public updateEventInfo(eventId: number, eventInfo: GeneralEventInformation): Observable<void> {
		const combinedDto = Object.assign({ eventId }, eventInfo);
		return this.httpClient.post<void>(this.baseUrl + '/UpdateEventInfo', combinedDto);
	}

	public changeOrganizer(eventId: number, newOrganizerId: number): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/ChangeOrganizer', { eventId, newOrganizerId });
	}

	public createEvent(eventInfo: GeneralEventInformation): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/CreateEvent', eventInfo);
	}

	public getEditDetails(eventId: number): Observable<EditEventDetails> {
		return this.httpClient.get<EditEventDetails>(this.baseUrl + '/GetEditDetails', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}
}
