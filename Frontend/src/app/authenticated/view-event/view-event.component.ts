import { Component } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { EventDetails } from '../../shared/server-model/event-details.model';
import { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';
import { DetailOverviewBase } from '../detail-overview-base';
import { AppointmentDetails } from '../../shared/server-model/event-edit-details.model';

@Component({
	selector: 'view-event',
	styleUrls: ['./view-event.component.scss'],
	templateUrl: './view-event.component.html'
})
export class ViewEventComponent extends DetailOverviewBase {
	public isOrganizingEvent: boolean;
	public eventDetails: EventDetails;

	private _eventId: number;

	public get eventId(): number {
		return this._eventId;
	}

	public set eventId(v: number) {
		this._eventId = v;
		this.loadEventDetails();
	}

	constructor(private snackBar: MatSnackBar,
				eventServer: ParticipateEventClient,
				dialog: MatDialog,
				authService: AuthService,
				route: ActivatedRoute) {
					super(eventServer, dialog, authService);
					route.params.subscribe(params => this.eventId = +params['id']);
				}


	public getAppointmentId(index: number, appointment: AppointmentDetails) {
		return appointment.appointmentInformation.appointmentId;
	}

	public async leaveEvent() {
		await this.leaveEventAsync(this.eventId);
		this.loadEventDetails();
	}

	public async joinEvent() {
		await this.joinEventAsync(this.eventId);
		this.loadEventDetails();
	}

	public setNotifications(notifications: NotificationConfiguration) {
		this.eventServer.configureNotifications(this.eventId, notifications).subscribe(
			() => this.snackBar.open('Notifikationen konfiguriert', 'Ok')
		);
	}

	public loadEventDetails() {
		this.eventServer.getDetails(this.eventId).subscribe(
			detail => {
				this.eventDetails = detail;
				this.isOrganizingEvent = this.currentUserId === detail.information.organizerId;
			},
			err => this.eventDetails = null);
	}
}
