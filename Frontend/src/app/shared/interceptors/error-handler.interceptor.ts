import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
	HttpErrorResponse
} from '@angular/common/http';
import { Router } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material';

import { Observable } from 'rxjs/Observable';
import { tap } from 'rxjs/operators';

import { AuthService } from './../services/auth.service';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

/**
 * Displays a generic error message whenever something regarding a HTTP request goes wrong
 */
@Injectable()
export class ErrorHandlerInterceptor implements HttpInterceptor {
	private currentlyOpenedDialog: MatDialogRef<ErrorDialogComponent>;

	constructor(private auth: AuthService, private router: Router, private dialog: MatDialog) {}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(request).pipe((
			tap(() => undefined, (event) => {
				if (event instanceof HttpErrorResponse) {
					console.warn('Received not OK response with status ' + event.status, event);
					if (event.status === 400) {
						// Ignore requests concerning wrong user input
						return;
					}

					// Unauthorized request (except on login) cause a redirect to the login
					if (event.status === 401 && request.url.endsWith('StartSession')) {
						this.auth.clearLocalSession();
						this.router.navigate(['Login']);
						return;
					}

					if (this.currentlyOpenedDialog) {
						// Prevent dialog spam
						this.currentlyOpenedDialog.close();
					}

					this.currentlyOpenedDialog = this.dialog.open(ErrorDialogComponent);
				}
		})));
	}
}
