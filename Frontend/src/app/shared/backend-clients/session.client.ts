import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { FrontendSession } from '../server-model/frontend-session.model';
import { ServerClientBase } from './server-client-base';

@Injectable({ providedIn: 'root' })
export class SessionClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('Session');
	}

	public getSession(sessionToken: string) {
		return this.httpClient.get<FrontendSession>(this.baseUrl + '/GetSession', {
			params: new HttpParams({ fromObject: { sessionToken } })
		});
	}

	public startSession(email: string, password: string) {
		return this.httpClient.post<FrontendSession>(this.baseUrl + '/StartSession', { email, password });
	}

	public stopActiveSession() {
		return this.httpClient.post<void>(this.baseUrl + '/StopActiveSession', null);
	}
}
