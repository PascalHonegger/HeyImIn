import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import { AuthService } from './../services/auth.service';
import { Observable } from 'rxjs';

/**
 * Appends the current session token to each request to authorize it
 */
@Injectable()
export class AppendSessionTokenInterceptor implements HttpInterceptor {

	constructor(private auth: AuthService) {}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		request = request.clone({
			setHeaders: {
				SessionToken: this.auth.sessionToken
			}
		});

		return next.handle(request);
	}
}
