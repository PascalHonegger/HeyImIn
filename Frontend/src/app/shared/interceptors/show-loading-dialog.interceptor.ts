import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import { finalize } from 'rxjs/operators';
import { MatDialog, MatDialogRef } from '@angular/material';
import { LoadingDialogComponent } from '../loading-dialog/loading-dialog.component';

/**
 * Displays a global loading dialog while a server request is running
 */
@Injectable()
export class ShowLoadingDialogInterceptor implements HttpInterceptor {
	private currentlyOpenedDialog: MatDialogRef<LoadingDialogComponent>;

	constructor(private dialog: MatDialog) {}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const openedDialog = this.dialog.open(LoadingDialogComponent, { disableClose: true });

		return next.handle(request).pipe(
			finalize(() => openedDialog.close())
		);
	}
}
