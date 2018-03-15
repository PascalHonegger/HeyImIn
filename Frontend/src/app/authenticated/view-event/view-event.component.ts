import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Constants } from '../../shared/constants';
import { UserClient } from '../../shared/backend-clients/user.client';
import { AuthService } from '../../shared/services/auth.service';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { Subscription } from 'rxjs/Subscription';
import { identifierModuleUrl } from '@angular/compiler';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { OrganizeAppointmentClient } from '../../shared/backend-clients/organize-appointment.client';
import { GeneralEventInfo } from '../../shared/server-model/general-event-info.model';
import { EventDetails } from '../../shared/server-model/event-details.model';
import { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';
import { DetailOverviewBase } from '../detail-overview-base';

@Component({
	selector: 'view-event',
	styleUrls: ['./view-event.component.scss'],
	templateUrl: './view-event.component.html'
})
export class ViewEventComponent extends DetailOverviewBase implements OnDestroy {
	public eventDetails: EventDetails;

	private subscription: Subscription;
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
					this.subscription = route.params.subscribe(params => this.eventId = +params['id']);
				}

	public ngOnDestroy() {
		this.subscription.unsubscribe();
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

	private loadEventDetails() {
		this.eventDetails = null;
		this.eventServer.getDetails(this.eventId).subscribe(
			detail => {
				this.eventDetails = detail;
			},
			err => this.eventDetails = null);
	}
}
