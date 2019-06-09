import { Component, Input, EventEmitter, Output, ChangeDetectionStrategy } from '@angular/core';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { AuthService } from '../../shared/services/auth.service';
import { DetailOverviewBase } from '../detail-overview-base';
import { MatDialog } from '@angular/material/dialog';
import { EventOverviewInformation } from '../../shared/server-model/event-overview-information.model';

@Component({
	selector: 'events-overview-list',
	styleUrls: ['./events-overview-list.component.scss'],
	templateUrl: './events-overview-list.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventsOverviewListComponent extends DetailOverviewBase {
	@Input()
	public events: readonly EventOverviewInformation[] = [];

	@Output()
	public eventChanged: EventEmitter<any> = new EventEmitter();

	constructor(eventServer: ParticipateEventClient,
				dialog: MatDialog,
				authService: AuthService) {
		super(eventServer, dialog, authService);
	}

	public getEventId(_index: number, event: EventOverviewInformation) {
		return event.eventId;
	}

	public async leaveEvent(eventId: number) {
		await this.leaveEventAsync(eventId);
		this.eventChanged.emit();
	}

	public async joinEvent(eventId: number) {
		await this.joinEventAsync(eventId);
		this.eventChanged.emit();
	}

	public currentUserDoesParticipate(event: EventOverviewInformation) {
		return event.viewEventInformation.participants.some(p => p.userId === this.currentSession.userId);
	}
}
