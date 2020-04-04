import { Injectable } from '@angular/core';
import type {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import type { AuthService } from '../services/auth.service';
import type { Observable } from 'rxjs';

/**
 * Appends the current session token to each request to authorize it
 */
@Injectable({ providedIn: 'root' })
export class AppendSessionTokenInterceptor implements HttpInterceptor {

	constructor(private auth: AuthService) {}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		request = request.clone({
			setHeaders: {
				SessionToken: this.auth.sessionToken || ''
			}
		});

		return next.handle(request);
	}
}
