import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import type {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';
import type { Router } from '@angular/router';
import type { MatDialog, MatDialogRef } from '@angular/material/dialog';
import type { MatSnackBar } from '@angular/material/snack-bar';

import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import type { AuthService } from '../services/auth.service';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

/**
 * Displays a generic error message whenever something regarding a HTTP request goes wrong
 */
@Injectable({ providedIn: 'root' })
export class ErrorHandlerInterceptor implements HttpInterceptor {
	private currentlyOpenedDialog?: MatDialogRef<ErrorDialogComponent>;

	constructor(private auth: AuthService, private router: Router, private dialog: MatDialog, private snackBar: MatSnackBar) {}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(request).pipe((
			tap(() => {}, (event) => {
				if (event instanceof HttpErrorResponse) {
					console.warn(event);
					if (event.status === 0) {
						// Display a toast when the server couldn't be reached
						this.snackBar.open('Mutterschiff konnte nicht erreicht werden', 'Ok');
						return;
					}

					if (event.status === 400) {
						// Display a toast with the error message
						// E.g. Email is already taken
						if (event.error) {
							this.snackBar.open(event.error, 'Ok');
						} else {
							this.snackBar.open('Unbekannter Validierungsfehler', 'Ok');
						}

						return;
					}

					if (event.status === 401) {
						// Unauthorized request (except on login) cause a redirect to the login
						if (!request.url.endsWith('StartSession')) {
							this.auth.clearLocalSession();
							this.router.navigate(['Login']);
						}
						return;
					}

					if (this.currentlyOpenedDialog !== undefined) {
						// Prevent dialog spam
						this.currentlyOpenedDialog.close();
					}

					this.currentlyOpenedDialog = this.dialog.open(ErrorDialogComponent);
				}
		})));
	}
}
