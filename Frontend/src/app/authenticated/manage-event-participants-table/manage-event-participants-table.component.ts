import { Component, ViewChild, Input, EventEmitter, Output } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import type { OnInit } from '@angular/core';
import type { MatDialog } from '@angular/material/dialog';
import type { ParticipateEventClient } from '../../shared/backend-clients/participate-event.client';
import type { UserInformation } from '../../shared/server-model/user-information.model';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';

@Component({
	selector: 'manage-event-participants-table',
	styleUrls: ['./manage-event-participants-table.component.scss'],
	templateUrl: './manage-event-participants-table.component.html'
})
export class ManageEventParticipantsTableComponent implements OnInit {
	@ViewChild(MatSort, { static: true })
	public sort: MatSort;

	public displayedColumns = ['name', 'email', 'action'];
	public dataSource = new MatTableDataSource<UserInformation>();

	// Forwards change event from appointment-participation
	@Output()
	public removedParticipant: EventEmitter<void> = new EventEmitter();

	@Input()
	public eventId: number;

	private _participants: readonly UserInformation[];
	@Input()
	public set participants(participants: readonly UserInformation[]) {
		this._participants = participants;
		this.dataSource.data = [...participants];
	}
	public get participants(): readonly UserInformation[] {
		return this._participants;
	}

	constructor(private dialog: MatDialog,
				private participateEventServer: ParticipateEventClient) { }

	public getUserId(_index: number, user: UserInformation): number {
		return user.userId;
	}

	public async removeFromEvent(participantId: number): Promise<void> {
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
	public ngOnInit(): void {
		this.dataSource.sort = this.sort;
	}
}
