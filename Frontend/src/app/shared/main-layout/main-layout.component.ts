// Taken from HappyMeter

import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Link } from './link.model';
import { UpdateService } from '../services/update.service';

@Component({
	selector: 'main-layout',
	templateUrl: 'main-layout.component.html',
	styleUrls: ['main-layout.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayoutComponent {
	@Input()
	public links: Link[];
	constructor(public updateService: UpdateService) { }
}
