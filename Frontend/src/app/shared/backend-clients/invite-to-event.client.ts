import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';

@Injectable({ providedIn: 'root' })
export class InviteToEventClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('InviteToEvent');
	}

	public inviteParticipants(eventId: number, emailAddresses: string[]) {
		return this.httpClient.post<void>(this.baseUrl + '/InviteParticipants', { eventId, emailAddresses });
	}

	public acceptInvitation(inviteToken: string) {
		return this.httpClient.post<number>(this.baseUrl + '/AcceptInvitation', { inviteToken });
	}
}
