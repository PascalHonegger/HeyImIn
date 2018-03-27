import { Component } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router, ActivatedRoute } from '@angular/router';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { OrganizeAppointmentClient } from '../../shared/backend-clients/organize-appointment.client';
import { OrganizeEventClient } from '../../shared/backend-clients/organize-event.client';
import { EditEventDetails } from '../../shared/server-model/event-edit-details.model';
import { GeneralEventInfo } from '../../shared/server-model/general-event-info.model';
import { AddAppointmentsDialogComponent } from '../add-appointments-dialog/add-appointments-dialog.component';
import { AddParticipantDialogComponent } from '../add-participant-dialog/add-participant-dialog.component';
import { InviteToEventClient } from '../../shared/backend-clients/invite-to-event.client';

@Component({
	selector: 'edit-event',
	styleUrls: ['./edit-event.component.scss'],
	templateUrl: './edit-event.component.html'
})
export class EditEventComponent {
	public eventDetails: EditEventDetails;

	private _eventId: number;

	public get eventId(): number {
		return this._eventId;
	}

	public set eventId(v: number) {
		this._eventId = v;
		this.loadEventDetails();
	}

	constructor(private inviteToEventServer: InviteToEventClient,
				private organizeEventServer: OrganizeEventClient,
				private organizeAppointmentServer: OrganizeAppointmentClient,
				private snackBar: MatSnackBar,
				private router: Router,
				private dialog: MatDialog,
				route: ActivatedRoute) {
					route.params.subscribe(params => this.eventId = +params['id']);
				}

	public saveEvent(newEventInfo: GeneralEventInfo) {
		this.organizeEventServer
			.updateEventInfo(this.eventId, newEventInfo)
			.subscribe(() => {
				this.eventDetails.information = newEventInfo;
				this.snackBar.open('Daten des Events aktualisiert', 'Ok');
			});
	}

	public async deleteEvent() {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie wirklich diesen Event und alle damit verbundenen Termine löschen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (result) {
			this.organizeEventServer.deleteEvent(this.eventId).subscribe(() => {
				this.snackBar.open('Event gelöscht', 'Ok');
				this.router.navigate(['/']);
			});
		}
	}

	public async addAppointments() {
		const newAppointments = await this.dialog
			.open(AddAppointmentsDialogComponent, {
				closeOnNavigation: true,
				width: '400px',
				minHeight: '400px'
			}).afterClosed().toPromise();

		if (newAppointments) {
			this.organizeAppointmentServer.addAppointments(this.eventId, newAppointments).subscribe(
				() => {
					// Reload data to include newly added appointments
					this.loadEventDetails();
				}
			);
		}
	}

	public async inviteParticipants() {
		const emailsToInvite = await this.dialog
			.open(AddParticipantDialogComponent, {
				closeOnNavigation: true,
				width: '400px',
				minHeight: '400px'
			}).afterClosed().toPromise();

		if (emailsToInvite) {
			this.inviteToEventServer.inviteParticipants(this.eventId, emailsToInvite).subscribe(
				() => {
					this.snackBar.open('Einladungen versendet', 'Ok');
				}
			);
		}
	}

	public async cancelAppointment(appointmentId: number) {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie diesen Termin wirklich absagen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

			if (result) {
				this.organizeAppointmentServer.deleteAppointment(appointmentId).subscribe(
					() => {
						// Remove appointment from local list
						this.eventDetails.upcomingAppointments = 
							this.eventDetails.upcomingAppointments.filter(u => u.appointmentInformation.appointmentId !== appointmentId);
					}
				);
			}
	}

	public loadEventDetails() {
		this.organizeEventServer.getEditDetails(this.eventId).subscribe(
			detail => {
				this.eventDetails = detail;
			},
			err => this.eventDetails = null);
	}
}
