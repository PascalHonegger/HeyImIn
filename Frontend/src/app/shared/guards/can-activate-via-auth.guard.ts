import { Injectable } from '@angular/core';
import type { Router, RouterStateSnapshot, ActivatedRouteSnapshot, CanActivateChild, CanActivate } from '@angular/router';

import type { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class CanActivateViaAuthGuard implements CanActivate, CanActivateChild {
	constructor(private authService: AuthService, private router: Router) {
	}

	public async canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
		// Ensure the user gets to the website he desired
		this.authService.urlAfterLogin = state.url;

		// Allow to pass a session token in any url as a queryparam
		// This enables acces links withing notification emails
		// E.g. www.website.com/AnySubSite?authToken=071DAF15-9AD9-4991-84F6-A6D104374C72
		const providedAuthToken = next.queryParams.authToken;

		if (providedAuthToken) {
			try {
				await this.authService.tryUpdateSession(providedAuthToken);
				return true;
			} catch (err) {
				console.warn('The provided authorization token was invalid, returning to local session', err);
			}
		}

		const localSessionValid = await this.authService.hasValidSession();

		if (localSessionValid) {
			return true;
		}

		this.router.navigate(['Login']);

		return false;
	}

	public canActivateChild(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
		return this.canActivate(next, state);
	}
}
