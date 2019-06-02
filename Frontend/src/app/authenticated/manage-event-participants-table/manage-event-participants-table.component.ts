import { Component, AfterViewInit, ViewChild, Input, EventEmitter, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import { UserInformation } from '../../shared/server-model/user-information.model';

@Component({
	selector: 'manage-event-participants-table',
	styleUrls: ['./manage-event-participants-table.component.scss'],
	templateUrl: './manage-event-participants-table.component.html'
})
export class ManageEventParticipantsTableComponent implements AfterViewInit {
	@ViewChild(MatSort, { static: false })
	public sort: MatSort;

	public displayedColumns = ['name', 'email', 'action'];
	public dataSource: MatTableDataSource<UserInformation>;

	// Forwards change event from appointment-participation
	@Output()
	public removedParticipant: EventEmitter<void> = new EventEmitter();

	@Input()
	public eventId: number;

	private _participants: UserInformation[];
	@Input()
	public set participants(participants: UserInformation[]) {
		this._participants = participants;
		this.dataSource = new MatTableDataSource(participants);
	}
	public get participants(): UserInformation[] {
		return this._participants;
	}

	constructor(private dialog: MatDialog,
				private participateEventServer: ParticipateEventClient) { }

	public getUserId(_index: number, user: UserInformation) {
		return user.userId;
	}

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
