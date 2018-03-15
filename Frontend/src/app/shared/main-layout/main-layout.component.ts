// Taken from HappyMeter

import { Component, Input } from '@angular/core';
import { Link } from './link.model';

@Component({
	selector: 'main-layout',
	templateUrl: 'main-layout.component.html',
	styleUrls: ['main-layout.component.scss']
})
export class MainLayoutComponent {
	@Input()
	public links: Link[];
}
