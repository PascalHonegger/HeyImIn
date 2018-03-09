// Taken from HappyMeter

import { Component, Input } from '@angular/core';
import { Link } from './link.model';

@Component({
	selector: 'navigation-toolbar',
	templateUrl: 'navigation-toolbar.component.html',
	styleUrls: ['navigation-toolbar.component.scss']
})
export class NavigationToolbarComponent {
	@Input()
	public links: Link[];
}
