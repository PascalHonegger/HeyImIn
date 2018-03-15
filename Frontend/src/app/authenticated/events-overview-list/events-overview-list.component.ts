import { Component, Input, EventEmitter, Output } from '@angular/core';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { EventOverview, EventOverviewInformation } from '../../shared/server-model/event-overview.model';
import { AuthService } from '../../shared/services/auth.service';
import { DetailOverviewBase } from '../detail-overview-base';
import { MatDialog } from '@angular/material';

@Component({
	selector: 'events-overview-list',
	styleUrls: ['./events-overview-list.component.scss'],
	templateUrl: './events-overview-list.component.html'
})
export class EventsOverviewListComponent extends DetailOverviewBase {
	@Input()
	public events: EventOverviewInformation[];

	@Output()
	public eventChanged: EventEmitter<any> = new EventEmitter();

	constructor(eventServer: ParticipateEventClient,
				dialog: MatDialog,
				authService: AuthService) {
		super(eventServer, dialog, authService);
	}

	public async leaveEvent(eventId: number) {
		await this.leaveEventAsync(eventId);
		this.eventChanged.emit();
	}

	public async joinEvent(eventId: number) {
		await this.joinEventAsync(eventId);
		this.eventChanged.emit();
	}
}
