import { Injectable } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { MatSnackBar } from '@angular/material';
import { interval, BehaviorSubject } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class UpdateService {
	public updateAvailable$ = new BehaviorSubject(false);

	constructor(private updates: SwUpdate, private snackBar: MatSnackBar) {
		if (!updates.isEnabled) {
			return;
		}

		updates.available.subscribe(() => this.updateAvailable$.next(true));

		interval(90 * 1000).subscribe(() => updates.checkForUpdate());
	}

	public async applyUpdate() {
		try {
			this.updateAvailable$.next(false);
			await this.updates.activateUpdate();
			// Reload the page to display newly activated version
			document.location.reload();
		} catch (error) {
			this.snackBar.open('Neue Version konnte nicht aktiviert werden', 'Ok');
			this.updateAvailable$.next(true);
		}
	}
}
