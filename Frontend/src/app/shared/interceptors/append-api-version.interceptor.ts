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
	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		request = request.clone({
			setParams: {
				'api-version': '2.0'
			}
		});

		return next.handle(request);
	}
}
