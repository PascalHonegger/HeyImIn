import { Component, AfterViewInit, ViewChild, Input, EventEmitter, Output, ChangeDetectionStrategy } from '@angular/core';
import { MatTableDataSource, MatSort } from '@angular/material';
import { AppointmentParticipationInformation, AppointmentInformation } from '../../shared/server-model/event-edit-details.model';
import { AuthService } from '../../shared/services/auth.service';

@Component({
	selector: 'appointment-participant-table',
	styleUrls: ['./appointment-participant-table.component.scss'],
	templateUrl: './appointment-participant-table.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppointmentParticipantTableComponent implements AfterViewInit {
	@ViewChild(MatSort)
	public sort: MatSort;

	public displayedColumns = ['participantName', 'response'];
	public dataSource: MatTableDataSource<AppointmentParticipationInformation>;

	// Forwards change event from appointment-participation
	@Output()
	public updatedResponse: EventEmitter<void> = new EventEmitter();

	@Input()
	public appointment: AppointmentInformation;

	@Input()
	public isOrganizingEvent: boolean;

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

	public get currentUserId(): number {
		return this.authService.session.userId;
	}

	constructor(private authService: AuthService) { }

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
