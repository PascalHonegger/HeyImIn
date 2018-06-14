import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { FrontendSession } from '../server-model/frontend-session.model';
import { ServerClientBase } from './server-client-base';

@Injectable({ providedIn: 'root' })
export class ResetPasswordClient extends ServerClientBase {
	constructor(private httpClient: HttpClient) {
		super('ResetPassword');
	}

	public requestPasswordReset(email: string) {
		return this.httpClient.post<void>(this.baseUrl + '/RequestPasswordReset', { email });
	}

	public resetPassword(passwordResetToken: string, newPassword: string) {
		return this.httpClient.post<FrontendSession>(this.baseUrl + '/ResetPassword', { passwordResetToken, newPassword });
	}
}
