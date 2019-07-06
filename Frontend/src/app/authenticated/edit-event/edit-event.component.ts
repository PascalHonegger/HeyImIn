import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { OrganizeAppointmentClient } from '../../shared/backend-clients/organize-appointment.client';
import { OrganizeEventClient } from '../../shared/backend-clients/organize-event.client';
import { AddAppointmentsDialogComponent } from '../add-appointments-dialog/add-appointments-dialog.component';
import { AddParticipantDialogComponent } from '../add-participant-dialog/add-participant-dialog.component';
import { InviteToEventClient } from '../../shared/backend-clients/invite-to-event.client';
import { AppointmentDetails } from '../../shared/server-model/appointment-details.model';
import { GeneralEventInformation } from '../../shared/server-model/general-event-information.model';
import { EditEventDetails } from '../../shared/server-model/edit-event-details.model';
import { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';
import {
	ChangeOrganizerDialogComponent,
	ChangeOrganizerDialogParameter
} from '../change-organizer-dialog/change-organizer-dialog.component';

@Component({
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
					route.params.subscribe(params => this.eventId = +params.id);
				}

	public getAppointmentId(_index: number, appointment: AppointmentDetails) {
		return appointment.appointmentId;
	}

	public setNewAnswer(appointment: AppointmentDetails,
		participantId: number,
		response: AppointmentParticipationAnswer) {
		appointment.participations = appointment.participations
			.filter(p => p.participantId !== participantId)
			.concat([{ participantId, response }]);
	}

	public saveEvent(newEventInfo: GeneralEventInformation) {
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
				minHeight: '0'
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
				minHeight: '0'
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
							this.eventDetails.upcomingAppointments.filter(u => u.appointmentId !== appointmentId);
					}
				);
			}
	}

	public async openChangeOrganizerDialog() {
		const newOrganizerId = await this.dialog
			.open<ChangeOrganizerDialogComponent, ChangeOrganizerDialogParameter, number | undefined>(ChangeOrganizerDialogComponent, {
				data: { participants: this.eventDetails.participants },
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (newOrganizerId) {
			this.organizeEventServer.changeOrganizer(this.eventId, newOrganizerId).subscribe(() => {
				this.snackBar.open('Der Organisator des Events wurde geändert', 'Ok');
				this.router.navigate(['/']);
			});
		}
	}

	public loadEventDetails() {
		this.organizeEventServer.getEditDetails(this.eventId).subscribe(
			detail => {
				this.eventDetails = detail;
			},
			_err => this.eventDetails = null);
	}
}
