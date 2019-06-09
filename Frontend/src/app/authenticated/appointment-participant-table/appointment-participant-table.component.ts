import { Component, AfterViewInit, ViewChild, Input, EventEmitter, Output, ChangeDetectionStrategy } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { AuthService } from '../../shared/services/auth.service';
import { AppointmentParticipationInformation } from '../../shared/server-model/appointment-participation-information.model';
import { UserInformation } from '../../shared/server-model/user-information.model';
import { AppointmentParticipationAnswer, NoAnswer } from '../../shared/server-model/appointment-participation-answer.model';
import { AppointmentDetails } from '../../shared/server-model/appointment-details.model';

@Component({
	selector: 'appointment-participant-table',
	styleUrls: ['./appointment-participant-table.component.scss'],
	templateUrl: './appointment-participant-table.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppointmentParticipantTableComponent implements AfterViewInit {
	@ViewChild(MatSort, { static: false })
	public sort: MatSort;

	public displayedColumns = ['name', 'response'];
	public dataSource: MatTableDataSource<UserInformation>;

	// Forwards change event from appointment-participation
	@Output()
	public updatedResponse: EventEmitter<{ participantId: number, newAnswer: AppointmentParticipationAnswer }> = new EventEmitter();

	@Input()
	public appointment: AppointmentDetails;

	@Input()
	public isOrganizingEvent: boolean;

	private _eventParticipants: readonly UserInformation[];
	@Input()
	public set eventParticipants(v: readonly UserInformation[]) {
		this._eventParticipants = v;
		this.dataSource = new MatTableDataSource(v.slice());
	}
	public get eventParticipants(): readonly UserInformation[] {
		return this._eventParticipants;
	}

	@Input()
	public givenAnswers: readonly AppointmentParticipationInformation[];

	public currentUserId: number;

	constructor(authService: AuthService) {
		this.currentUserId = authService.session.userId;
	}

	public getUserId(_index: number, participation: UserInformation) {
		return participation.userId;
	}

	public findGivenAnswer(userId: number): AppointmentParticipationAnswer | undefined {
		const foundAnswer = this.givenAnswers.find(a => a.participantId === userId);
		return foundAnswer !== undefined ? foundAnswer.response : NoAnswer;
	}

	public setResponse(participantId: number, newAnswer: AppointmentParticipationAnswer) {
		this.updatedResponse.emit({ participantId, newAnswer });
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
