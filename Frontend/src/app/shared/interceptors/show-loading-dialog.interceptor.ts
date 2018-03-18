import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpHandler,
	HttpEvent,
	HttpInterceptor
} from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import { finalize, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { MatDialog, MatDialogRef } from '@angular/material';
import { LoadingDialogComponent } from '../loading-dialog/loading-dialog.component';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

/**
 * Displays a global loading dialog while a server request is running
 */
@Injectable()
export class ShowLoadingDialogInterceptor implements HttpInterceptor {
	private currentlyOpenedDialog: MatDialogRef<LoadingDialogComponent>;

	private hasDialogOpen: BehaviorSubject<number> = new BehaviorSubject(0);

	private _runningRequests: number = 0;
	private get runningRequests(): number {
		return this._runningRequests;
	}
	private set runningRequests(value: number) {
		this._runningRequests = value;
		this.hasDialogOpen.next(value);
	}

	constructor(private dialog: MatDialog) {
		this.hasDialogOpen
			.pipe(
				map(value => value > 0),
				debounceTime(200),
				distinctUntilChanged()
			)
			.subscribe(
				(open) => {
					if (open) {
						this.currentlyOpenedDialog = this.dialog.open(LoadingDialogComponent, { disableClose: true });
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
