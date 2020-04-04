import { Injectable } from '@angular/core';
import type { FrontendSession } from '../server-model/frontend-session.model';
import type { SessionClient } from '../backend-clients/session.client';

const sessionTokenStorageKey = 'sessionToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
	/**
	 * The route to navigate to after login / registration
	 * Will be set and used externally
	 */
	public urlAfterLogin = '/';

	private _sessionToken: string | null = null;
	private _session: FrontendSession = null;

	constructor(private server: SessionClient) {
		this._sessionToken = localStorage.getItem(sessionTokenStorageKey) || '';
	}

	public get sessionToken(): string | null {
		return this._sessionToken;
	}

	public set sessionToken(value: string | null) {
		if (this._sessionToken === value) {
			return;
		}

		this._sessionToken = value;
		if (value) {
			localStorage.setItem(sessionTokenStorageKey, value);
		} else {
			localStorage.removeItem(sessionTokenStorageKey);
		}
	}

	public get session(): FrontendSession | null {
		return this._session;
	}

	public set session(value: FrontendSession) {
		this._session = value;
		this.sessionToken = value.token;
	}

	/**
	 * Tries to load session information for the provided token
	 * If the token is valid the local session gets overriden
	 * @throws An exception if the provided token is invalid
	 * @param sessionToken Token to load for
	 */
	public async tryUpdateSession(sessionToken: string): Promise<void> {
		const loadedSession = await this.server.getSession(sessionToken).toPromise();
		this.session = loadedSession;
	}

	/**
	 * Tries to create a session, which would therefor log the user in
	 * @throws An exception if the provided credentials are invalid
	 * @param email Email to log in with
	 * @param password Password to log in with
	 */
	public async tryCreateSession(email: string, password: string): Promise<void> {
		const createdSession = await this.server.startSession(email, password).toPromise();
		this.session = createdSession;
	}

	/**
	 * Checks if the locally saved session token is valid
	 * If necessary loads session information from the backend
	 */
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
			return false;
		}
	}

	/**
	 * Logs the current user out by invalidating his session on the server
	 * The local session gets destory, whether the server response is successful or not
	 */
	public async logOut(): Promise<void> {
		try {
			await this.server.stopActiveSession().toPromise();
			// Prevent redirect to profile after logout
			this.urlAfterLogin = '/';
		} catch (err) {
			// The server session will automatically turn invalid after a while
		}

		this.clearLocalSession();
	}

	/**
	 * Deletes the lcaolly saved information about the session, effectively logging the user out
	 */
	public clearLocalSession() {
		this._sessionToken = null;
		sessionStorage.removeItem(sessionTokenStorageKey);
	}
}
