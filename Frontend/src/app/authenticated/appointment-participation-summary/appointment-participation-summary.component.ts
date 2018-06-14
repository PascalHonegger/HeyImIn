import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { AppointmentInformation } from '../../shared/server-model/event-edit-details.model';

@Component({
	selector: 'appointment-participation-summary',
	templateUrl: 'appointment-participation-summary.component.html',
	styleUrls: ['appointment-participation-summary.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppointmentParticipationSummaryComponent {
	@Input()
	public appointment: AppointmentInformation;

	@Input()
	public orientation = 'column';
}
