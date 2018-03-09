import { Injectable } from '@angular/core';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot, CanActivateChild, CanActivate } from '@angular/router';

import { Observable } from 'rxjs/Observable';

@Injectable()
export class CanActivateViaAuthGuard implements CanActivate, CanActivateChild {
	public async canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
		// TODO Implement

		return true;
	}

	public canActivateChild(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
		return this.canActivate(next, state);
	}
}
