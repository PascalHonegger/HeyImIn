import { Component, Input, ChangeDetectionStrategy, OnChanges, SimpleChanges } from '@angular/core';
import { UserInformation } from '../../shared/server-model/user-information.model';
import { Accepted, Declined } from '../../shared/server-model/appointment-participation-answer.model';
import { AppointmentParticipationInformation } from '../../shared/server-model/appointment-participation-information.model';

@Component({
	selector: 'appointment-participation-summary',
	templateUrl: 'appointment-participation-summary.component.html',
	styleUrls: ['appointment-participation-summary.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppointmentParticipationSummaryComponent implements OnChanges {
	@Input()
	public eventParticipants: ReadonlyArray<UserInformation>;

	@Input()
	public givenAnswers: AppointmentParticipationInformation[];

	@Input()
	public orientation = 'column';

	public acceptedCount = 0;
	public declinedCount = 0;
	public noAnswerCount = 0;

	public ngOnChanges(_changes: SimpleChanges): void {
		this.acceptedCount = this.givenAnswers.filter(a => a.response === Accepted).length;
		this.declinedCount = this.givenAnswers.filter(a => a.response === Declined).length;
		this.noAnswerCount = this.eventParticipants.length - (this.acceptedCount + this.declinedCount);
	}
}
