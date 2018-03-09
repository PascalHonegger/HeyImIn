import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { catchError, map, finalize } from 'rxjs/operators';
import { FrontendSession } from '../server-model/frontend-session.model';
import { SessionClient } from '../backend-clients/session.client';

const sessionTokenStorageKey = 'sessionToken';

@Injectable()
export class AuthService {
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

	public async updateSessionIfValid(sessionToken: string): Promise<boolean> {
		// Load session from server
		// If success update local session
		// Return wheter it was a success or not
		return true;
	}

	public async tryCreateSession(email: string, password: string): Promise<boolean> {
		// Create session from credentials
		// If success apply session
		// Return if it was a success

		return true;
	}

	public async hasValidSession(): Promise<boolean> {
		if (!this.sessionToken) {
			return false;
		}

		if (this.session) {
			return true;
		}

		// We have a saved token but no session => Check if the saved token is still valid

		// Load session information from server
		// If success apply session
		// Return if it was a success
	}

	public async logOut(): Promise<void> {
		// Remove session in server
		this.clearLocalSession();
	}

	public clearLocalSession() {
		this._sessionToken = '';
		sessionStorage.removeItem(sessionTokenStorageKey);
	}
}
