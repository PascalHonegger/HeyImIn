import { Component } from '@angular/core';

import type { Link } from '../../shared/main-layout/link.model';

@Component({
	styleUrls: [ './anonymous-layout.component.scss' ],
	templateUrl: './anonymous-layout.component.html'
})
export class AnonymousLayoutComponent {
	public links: Link[] = [{
		url: ['/Login'],
		matIcon: 'input',
		content: 'Anmelden'
	}, {
		url: ['/Register'],
		matIcon: 'person_add',
		content: 'Registrieren'
	}];
}
