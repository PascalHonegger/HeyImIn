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

@Component({
	selector: 'view-event',
	styleUrls: ['./view-event.component.scss'],
	templateUrl: './view-event.component.html'
})
export class ViewEventComponent implements OnDestroy {
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

	public get currentUserId(): number {
		return this.authService.session.userId;
	}

	constructor(private eventServer: ParticipateEventClient,
				private organizeAppointmentServer: OrganizeAppointmentClient,
				private snackBar: MatSnackBar,
				private dialog: MatDialog,
				private authService: AuthService,
				route: ActivatedRoute) {
					this.subscription = route.params.subscribe(params => this.eventId = +params['id']);
				}

	public ngOnDestroy() {
		this.subscription.unsubscribe();
	}

	public async leaveEvent() {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie diesen Event und alle damit verbundenen Termine verlassen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (result) {
			this.eventServer.removeFromEvent(this.eventId, this.currentUserId).subscribe(() => {
				this.snackBar.open('Event verlassen', 'Ok');

				// Reload details
				this.loadEventDetails();
			});
		}
	}

	public joinEvent() {
		this.eventServer.joinEvent(this.eventId).subscribe(() => {
			// Reload details
			this.loadEventDetails();
		});
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
