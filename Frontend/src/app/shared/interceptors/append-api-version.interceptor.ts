import { Injectable } from '@angular/core';
import type {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import type { Observable } from 'rxjs';

/**
 * Appends the current api version as a query parameter to each request
 */
@Injectable({ providedIn: 'root' })
export class AppendApiVersionInterceptor implements HttpInterceptor {
	public intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
		request = request.clone({
			setParams: {
				'api-version': '2.0'
			}
		});

		return next.handle(request);
	}
}
