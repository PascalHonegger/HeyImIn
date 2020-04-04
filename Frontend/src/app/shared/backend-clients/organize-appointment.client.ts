import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { AppointmentParticipationAnswer } from '../server-model/appointment-participation-answer.model';

@Injectable({ providedIn: 'root' })
export class OrganizeAppointmentClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('OrganizeAppointment');
	}

	public deleteAppointment(appointmentId: number) {
		return this.httpClient.delete<void>(this.baseUrl + '/DeleteAppointment', {
			params: new HttpParams({ fromObject: { appointmentId: appointmentId.toString() } })
		});
	}

	public addAppointments(eventId: number, startTimes: readonly (Date | string)[]) {
		return this.httpClient.post<void>(this.baseUrl + '/AddAppointments', { eventId, startTimes });
	}

	public setAppointmentResponse(appointmentId: number, userId: number, response?: AppointmentParticipationAnswer) {
		return this.httpClient.post<void>(this.baseUrl + '/SetAppointmentResponse', { appointmentId, userId, response });
	}
}
