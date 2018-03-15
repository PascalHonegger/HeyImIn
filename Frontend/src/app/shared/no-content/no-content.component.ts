import { Component } from '@angular/core';
import { Link } from '../main-layout/link.model';

@Component({
	selector: 'no-content',
	styleUrls: ['./no-content.component.scss'],
	templateUrl: './no-content.component.html'
})
export class NoContentComponent {
	public links: Link[] = [{
		url: ['/'],
		matIcon: 'home',
		content: 'Startseite'
	}];
}
