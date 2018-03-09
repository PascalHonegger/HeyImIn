import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { catchError, map, finalize } from 'rxjs/operators';
import { FrontendSession } from '../server-model/frontend-session.model';
import { SessionClient } from '../backend-clients/session.client';

const sessionTokenStorageKey = 'sessionToken';

@Injectable()
export class AuthService {
	public urlAfterLogin: string = '/';

	private _sessionToken: string;
	private _session: FrontendSession = null;

	constructor(private server: SessionClient) {
		this._sessionToken = localStorage.getItem(sessionTokenStorageKey) || '';
	}

	public get sessionToken(): string {
		return this._sessionToken;
	}

	public set sessionToken(value: string) {
		if (this._sessionToken === value) {
			return;
		}

		this._sessionToken = value;
		localStorage.setItem(sessionTokenStorageKey, value);
	}

	public get session(): FrontendSession | null {
		return this._session;
	}

	public set session(value: FrontendSession) {
		this._session = value;
		this.sessionToken = value.token;
	}

	public async tryUpdateSession(sessionToken: string): Promise<void> {
		const loadedSession = await this.server.getSession(sessionToken).toPromise();
		this.session = loadedSession;
	}

	public async tryCreateSession(email: string, password: string): Promise<void> {
		const createdSession = await this.server.startSession(email, password).toPromise();
		this.session = createdSession;
	}

	public async hasValidSession(): Promise<boolean> {
		if (!this.sessionToken) {
			return false;
		}

		if (this.session) {
			return true;
		}

		// We have a saved token but no session => Check if the saved token is still valid

		try {
			const loadedSession = await this.server.getSession(this.sessionToken).toPromise();
			this.session = loadedSession;
			return true;
		} catch (err) {
			// If the error was due to the token being invalid (401),
			// an interceptor will clear the local session automatically
			console.info('Loading saved session failed', err);
		}
	}

	public async logOut(): Promise<void> {
		try {
			await this.server.stopActiveSession().toPromise();
		} catch (err) {
			// The server session will automatically turn invalid after a while
			console.info('Removing session from server failed', err);
		}

		this.clearLocalSession();
	}

	public clearLocalSession() {
		this._sessionToken = '';
		sessionStorage.removeItem(sessionTokenStorageKey);
	}
}
