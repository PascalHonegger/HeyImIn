import { Injectable } from '@angular/core';
import type { HttpClient } from '@angular/common/http';

import { ServerClientBase } from './server-client-base';
import type { FrontendSession } from '../server-model/frontend-session.model';
import type { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ResetPasswordClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('ResetPassword');
	}

	public requestPasswordReset(email: string): Observable<void> {
		return this.httpClient.post<void>(this.baseUrl + '/RequestPasswordReset', { email });
	}

	public resetPassword(passwordResetToken: string, newPassword: string): Observable<FrontendSession> {
		return this.httpClient.post<FrontendSession>(this.baseUrl + '/ResetPassword', { passwordResetToken, newPassword });
	}
}
