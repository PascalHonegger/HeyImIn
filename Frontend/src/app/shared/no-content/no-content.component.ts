import { Component, ChangeDetectionStrategy } from '@angular/core';
import type { Link } from '../main-layout/link.model';

@Component({
	styleUrls: ['./no-content.component.scss'],
	templateUrl: './no-content.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class NoContentComponent {
	public links: Link[] = [{
		url: ['/'],
		matIcon: 'home',
		content: 'Startseite'
	}];
}
