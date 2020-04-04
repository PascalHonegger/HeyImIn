import { Component } from '@angular/core';
import type { Router } from '@angular/router';
import type { OrganizeEventClient } from '../../shared/backend-clients/organize-event.client';
import type { GeneralEventInformation } from '../../shared/server-model/general-event-information.model';

@Component({
	styleUrls: ['./create-event.component.scss'],
	templateUrl: './create-event.component.html'
})
export class CreateEventComponent {
	public defaultEventInfo: GeneralEventInformation = {
		title: '',
		meetingPlace: '',
		description: '',
		isPrivate: false,
		reminderTimeWindowInHours: 48,
		summaryTimeWindowInHours: 4
	};

	constructor(private organizeEventServer: OrganizeEventClient,
				private router: Router) { }

	public createEvent(newEventInfo: GeneralEventInformation) {
		this.organizeEventServer
			.createEvent(newEventInfo)
			.subscribe((id) => this.router.navigate(['/EditEvent', id]));
	}
}
