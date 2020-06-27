import { Injectable } from '@angular/core';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class InviteToEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('InviteToEvent');
	}

	public inviteParticipants(eventId: number, emailAddresses: string[]): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/InviteParticipants', { eventId, emailAddresses });
	}

	public acceptInvitation(inviteToken: string): Observable<number> {
		return this.httpClient.post<number>(this.baseUrl + '/AcceptInvitation', { inviteToken });
	}
}
