import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { OrganizeAppointmentClient } from '../../shared/backend-clients/organize-appointment.client';
import { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';

@Component({
	selector: 'appointment-participation',
	styleUrls: ['./appointment-participation.component.scss'],
	templateUrl: './appointment-participation.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppointmentParticipationComponent {
	@Input()
	public currentResponse?: AppointmentParticipationAnswer;
	@Input()
	public readonly: boolean;
	@Input()
	public appointmentId: number;
	@Input()
	public participantId: number;

	@Output()
	public selectedNewResponse: EventEmitter<void> = new EventEmitter();

	public get accepted() {
		return this.currentResponse === AppointmentParticipationAnswer.Accepted;
	}
	public get declined() {
		return this.currentResponse === AppointmentParticipationAnswer.Declined;
	}
	public get noAnswer() {
		return this.currentResponse === undefined;
	}

	constructor(private server: OrganizeAppointmentClient) { }

	public accept(event: MouseEvent) {
		this.clickedResponse(event, AppointmentParticipationAnswer.Accepted);
	}
	public decline(event: MouseEvent) {
		this.clickedResponse(event, AppointmentParticipationAnswer.Declined);
	}
	public removeAnswer(event: MouseEvent) {
		this.clickedResponse(event, undefined);
	}

	public clickedResponse(event: MouseEvent, clicked?: AppointmentParticipationAnswer) {
		// Prevent any elements like expansion panels to trigger
		event.stopPropagation();

		if (this.readonly) {
			return;
		}

		if (clicked !== this.currentResponse) {
			this.server.setAppointmentResponse(this.appointmentId, this.participantId, clicked).subscribe(
				() => this.selectedNewResponse.emit()
			);
		}
	}
}
