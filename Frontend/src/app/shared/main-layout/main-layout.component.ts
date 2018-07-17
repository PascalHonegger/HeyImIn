// Taken from HappyMeter

import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Link } from './link.model';
import { UpdateService } from '../services/update.service';
import { Observable } from 'rxjs';
import { MatSnackBar } from '@angular/material';
import { tap } from 'rxjs/operators';

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

	public applyUpdate() {
		this.updateService.applyUpdate();
	}
}
