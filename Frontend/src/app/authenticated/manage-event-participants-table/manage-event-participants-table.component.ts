import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, Input, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar, MatDialog, MatTableDataSource, MatSort } from '@angular/material';
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
import { OrganizeEventClient } from '../../shared/backend-clients/organize-event.client';
import { EditEventDetails, AppointmentParticipationInformation, AppointmentInformation, EventParticipantInformation } from '../../shared/server-model/event-edit-details.model';
import { GeneralEventInfo } from '../../shared/server-model/general-event-info.model';
import { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';

@Component({
	selector: 'manage-event-participants-table',
	styleUrls: ['./manage-event-participants-table.component.scss'],
	templateUrl: './manage-event-participants-table.component.html'
})
export class ManageEventParticipantsTableComponent implements AfterViewInit {
	@ViewChild(MatSort)
	public sort: MatSort;

	public displayedColumns = ['participantName', 'participantEmail', 'action'];
	public dataSource: MatTableDataSource<EventParticipantInformation>;

	// Forwards change event from appointment-participation
	@Output()
	public removedParticipant: EventEmitter<void> = new EventEmitter();

	@Input()
	public eventId: number;

	private _participants: EventParticipantInformation[];
	@Input()
	public set participants(v: EventParticipantInformation[]) {
		this._participants = v;
		this.dataSource = new MatTableDataSource(v);
		this.dataSource.sort = this.sort;
	}
	public get participants(): EventParticipantInformation[] {
		return this._participants;
	}

	constructor(private dialog: MatDialog,
				private participateEventServer: ParticipateEventClient) { }

	public async removeFromEvent(participantId: number) {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie diesen Benutzer wirklich vom Event und allen Terminen entfernen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

			if (result) {
				this.participateEventServer.removeFromEvent(this.eventId, participantId).subscribe(
					() => this.removedParticipant.emit()
				);
			}
	}

	/**
	 * Set the sort after the view init since this component will
	 * be able to query its view for the initialized sort.
	 */
	public ngAfterViewInit() {
		if (this.dataSource) {
			this.dataSource.sort = this.sort;
		}
	}
}
