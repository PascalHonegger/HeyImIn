import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { FrontendSession } from '../server-model/frontend-session.model';
import { ServerClientBase } from './server-client-base';
import { AppointmentParticipationAnswer } from '../server-model/appointment-participation-answer.model';
import { GeneralEventInfo } from '../server-model/general-event-info.model';
import { EditEventDetails } from '../server-model/event-edit-details.model';

@Injectable()
export class OrganizeEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('OrganizeEvent');
	}

	public deleteEvent(eventId: number) {
		return this.httpClient.delete<void>(this.baseUrl + '/DeleteEvent', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}

	public updateEventInfo(eventId: number, eventInfo: GeneralEventInfo) {
		const combinedDto = Object.assign({ eventId }, eventInfo);
		return this.httpClient.post<void>(this.baseUrl + '/UpdateEventInfo', combinedDto);
	}

	public createEvent(eventInfo: GeneralEventInfo) {
		return this.httpClient.post<void>(this.baseUrl + '/CreateEvent', eventInfo);
	}

	public getEditDetails(eventId: number) {
		return this.httpClient.get<EditEventDetails>(this.baseUrl + '/GetEditDetails', {
			params: new HttpParams({ fromObject: { eventId: eventId.toString() } })
		});
	}
}
