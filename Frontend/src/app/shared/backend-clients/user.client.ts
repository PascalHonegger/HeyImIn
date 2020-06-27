import { Injectable } from '@angular/core';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { FrontendSession } from '../server-model/frontend-session.model';
import type { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('User');
	}

	public setNewUserData(newFullName: string, newEmail: string): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/SetNewUserData', { newFullName, newEmail });
	}

	public setNewPassword(currentPassword: string, newPassword: string): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/SetNewPassword', { currentPassword, newPassword });
	}

	public deleteAccount(): Observable<void> {
		return this.httpClient.delete<void>(this.baseUrl + '/DeleteAccount');
	}

	public register(fullName: string, email: string, password: string): Observable<FrontendSession> {
		return this.httpClient.post<FrontendSession>(this.baseUrl + '/Register', { fullName, email, password });
	}
}
