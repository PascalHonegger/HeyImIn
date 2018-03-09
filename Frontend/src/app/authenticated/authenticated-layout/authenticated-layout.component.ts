import { Component } from '@angular/core';

import { Link } from '../../shared/navigation-toolbar/link.model';

@Component({
	selector: 'authenticated-layout',
	styleUrls: [ './authenticated-layout.component.scss' ],
	templateUrl: './authenticated-layout.component.html'
})
export class AuthenticatedLayoutComponent {
	public links: Link[] = [{
		url: ['/Events'],
		matIcon: 'event',
		content: 'Events'
	}, {
		url: ['/CreateEvent'],
		matIcon: 'event_note',
		content: 'Neuer Event'
	}, {
		url: ['/Profile'],
		matIcon: 'account_circle',
		content: null
	}];
}
