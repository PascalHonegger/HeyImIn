import { Component } from '@angular/core';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { EventOverview } from '../../shared/server-model/event-overview.model';

@Component({
	selector: 'events-overview',
	styleUrls: ['./events-overview.component.scss'],
	templateUrl: './events-overview.component.html'
})
export class EventsOverviewComponent {
	public eventOverview: EventOverview;

	constructor(private server: ParticipateEventClient) {
		this.loadEventOverview();
	}

	public loadEventOverview() {
		this.server.getOverview().subscribe(
			(overview) => this.eventOverview = overview
		);
	}
}
