import { Injectable } from '@angular/core';
import type { SwUpdate } from '@angular/service-worker';
import type { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class UpdateService {
	public updateAvailable$ = new BehaviorSubject(false);

	constructor(private updates: SwUpdate, private snackBar: MatSnackBar) {
		if (!updates.isEnabled) {
			console.warn('Service Workers not enabled / supported');
			return;
		}

		updates.available.subscribe(() => this.updateAvailable$.next(true));

		/* This breaks because of Angular issue https://github.com/angular/angular/issues/20970
		interval(90 * 1000).subscribe(async () => {
			try {
				await updates.checkForUpdate();
			} catch (e) {
				console.warn('Failed to check for updates', e);
			}
		});
		*/
	}

	public async applyUpdate(): Promise<void> {
		try {
			this.updateAvailable$.next(false);
			await this.updates.activateUpdate();
			// Reload the page to display newly activated version
			document.location.reload();
		} catch (error) {
			this.updateAvailable$.next(true);
			this.snackBar.open('Neue Version konnte nicht aktiviert werden', 'Ok');
		}
	}
}
