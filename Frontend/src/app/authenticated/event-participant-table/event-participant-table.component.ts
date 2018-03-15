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
import { EditEventDetails, AppointmentParticipationInformation, AppointmentInformation } from '../../shared/server-model/event-edit-details.model';
import { GeneralEventInfo } from '../../shared/server-model/general-event-info.model';
import { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';

@Component({
	selector: 'event-participant-table',
	styleUrls: ['./event-participant-table.component.scss'],
	templateUrl: './event-participant-table.component.html'
})
export class EventParticipantTableComponent implements AfterViewInit {
	@ViewChild(MatSort)
	public sort: MatSort;

	public displayedColumns = ['participantName', 'response'];
	public dataSource: MatTableDataSource<AppointmentParticipationInformation>;

	// Forwards change event from event-participation
	@Output()
	public updatedResponse: EventEmitter<void> = new EventEmitter();

	@Input()
	public allowChange: boolean;

	@Input()
	public appointment: AppointmentInformation;

	private _participants: AppointmentParticipationInformation[];
	@Input()
	public set participants(v: AppointmentParticipationInformation[]) {
		this._participants = v;
		this.dataSource = new MatTableDataSource(v);
		this.dataSource.sort = this.sort;
	}
	public get participants(): AppointmentParticipationInformation[] {
		return this._participants;
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
