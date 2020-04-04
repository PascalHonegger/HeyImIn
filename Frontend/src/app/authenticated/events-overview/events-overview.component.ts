import { Component } from '@angular/core';
import type { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import type { EventOverview } from '../../shared/server-model/event-overview.model';

@Component({
	styleUrls: ['./events-overview.component.scss'],
	templateUrl: './events-overview.component.html'
})
export class EventsOverviewComponent {
	public eventOverview: EventOverview | null = null;

	constructor(private server: ParticipateEventClient) {
		this.loadEventOverview();
	}

	public loadEventOverview() {
		this.server.getOverview().subscribe(
			overview => this.eventOverview = overview,
			_err => this.eventOverview = null
		);
	}
}
