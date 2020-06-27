import { Component } from '@angular/core';
import type { MatDialog } from '@angular/material/dialog';
import type { MatSnackBar } from '@angular/material/snack-bar';
import type { ActivatedRoute } from '@angular/router';
import type { AuthService } from '../../shared/services/auth.service';
import type { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import type { EventDetails } from '../../shared/server-model/event-details.model';
import type { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';
import type { AppointmentDetails } from '../../shared/server-model/appointment-details.model';
import type { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';
import { DetailOverviewBase } from '../detail-overview-base';

@Component({
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
					route.params.subscribe(params => this.eventId = +params.id);
				}

	public getAppointmentId(_index: number, appointment: AppointmentDetails): number {
		return appointment.appointmentId;
	}

	public async leaveEvent(): Promise<void> {
		await this.leaveEventAsync(this.eventId);
		this.loadEventDetails();
	}

	public async joinEvent(): Promise<void> {
		await this.joinEventAsync(this.eventId);
		this.eventDetails.information.participants = this.eventDetails.information.participants.concat([this.currentUserInformation]);
	}

	public setNotifications(notifications: NotificationConfiguration): void {
		this.eventServer.configureNotifications(this.eventId, notifications).subscribe(
			() => this.snackBar.open('Notifikationen konfiguriert', 'Ok')
		);
	}

	public loadEventDetails(): void {
		this.eventServer.getDetails(this.eventId).subscribe(
			detail => {
				this.eventDetails = detail;
				this.isOrganizingEvent = this.currentSession.userId === detail.information.organizer.userId;
			},
			_err => this.eventDetails = null);
	}

	public setNewAnswer(appointment: AppointmentDetails,
						participantId: number,
						response: AppointmentParticipationAnswer): void {
		if (!this.currentUserDoesParticipate) {
			// We could theoretically optimize this, but too much effort regarding change detection
			this.loadEventDetails();
			return;

			// This can logically only happen to the current user
			// Also add the user to the event participations
			// this.eventDetails.information.participants = this.eventDetails.information.participants.concat([this.currentUserInformation]);
		}

		appointment.participations = appointment.participations
			.filter(p => p.participantId !== participantId)
			.concat([{ participantId, response }]);
	}

	public get currentUserDoesParticipate(): boolean {
		return this.eventDetails && this.eventDetails.information.participants.some(p => p.userId === this.currentSession.userId);
	}
}
