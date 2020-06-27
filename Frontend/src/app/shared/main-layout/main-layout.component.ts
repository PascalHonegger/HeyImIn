// Taken from HappyMeter

import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import type { MatSnackBar } from '@angular/material/snack-bar';
import type { Link } from './link.model';
import type { UpdateService } from '../services/update.service';

@Component({
	selector: 'main-layout',
	templateUrl: 'main-layout.component.html',
	styleUrls: ['main-layout.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayoutComponent {
	@Input()
	public links: Link[] = [];

	public updateAvailable$: Observable<boolean>;

	constructor(private updateService: UpdateService, snackBar: MatSnackBar) {
		this.updateAvailable$ = this.updateService.updateAvailable$.pipe(
			tap(available => {
				if (available) {
					snackBar.open('Update verfügbar', 'Aktualisieren').onAction().subscribe(() => this.applyUpdate());
				}
			})
		);
	}

	public applyUpdate(): void {
		this.updateService.applyUpdate();
	}
}
