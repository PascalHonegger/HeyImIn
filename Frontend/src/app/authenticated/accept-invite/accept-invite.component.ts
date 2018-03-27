import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import { InviteToEventClient } from '../../shared/backend-clients/invite-to-event.client';

@Component({
	selector: 'accept-invite',
	styleUrls: ['./accept-invite.component.scss'],
	templateUrl: './accept-invite.component.html'
})
export class AcceptInviteComponent {
	public messageToDisplay: string = 'Ungültiger Link';

	private subscription: Subscription;

	constructor(server: InviteToEventClient, route: ActivatedRoute, router: Router) {
		route.params.subscribe(params => {
			const token = params['token'];
			this.messageToDisplay = 'Einladung wird angenommen...';
			server.acceptInvitation(token).subscribe(
				id => {
					this.messageToDisplay = 'Sie werden weitergeleitet...';
					router.navigate(['/ViewEvent', id]);
				}
			);
		});
	}
}
