import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import { Observable, Subject } from 'rxjs';
import { finalize, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { MatDialog, MatDialogRef } from '@angular/material';
import { LoadingDialogComponent } from '../loading-dialog/loading-dialog.component';

/**
 * Displays a global loading dialog while a server request is running
 */
@Injectable({ providedIn: 'root' })
export class ShowLoadingDialogInterceptor implements HttpInterceptor {
	private currentlyOpenedDialog: MatDialogRef<LoadingDialogComponent>;

	private runningRequestsSource: Subject<number> = new Subject<number>();

	private _runningRequests = 0;
	private get runningRequests(): number {
		return this._runningRequests;
	}
	private set runningRequests(value: number) {
		this._runningRequests = value;
		this.runningRequestsSource.next(value);
	}

	constructor(private dialog: MatDialog) {
		this.runningRequestsSource
			.pipe(
				map(value => value > 0),
				debounceTime(400),
				distinctUntilChanged()
			)
			.subscribe(
				(open) => {
					if (open) {
						this.currentlyOpenedDialog = this.dialog.open(LoadingDialogComponent, { disableClose: true, closeOnNavigation: false });
					} else if (this.currentlyOpenedDialog) {
						this.currentlyOpenedDialog.close();
					}
				}
		);
	}

	public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		this.runningRequests++;

		return next.handle(request).pipe(
			finalize(() => this.runningRequests--)
		);
	}
}
