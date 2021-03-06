import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import type { OrganizeAppointmentClient } from '../../shared/backend-clients/organize-appointment.client';
import type { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';
import { Accepted, Declined, NoAnswer } from '../../shared/server-model/appointment-participation-answer.model';

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
	public selectedNewResponse: EventEmitter<AppointmentParticipationAnswer> = new EventEmitter();

	public get accepted(): boolean {
		return this.currentResponse === Accepted;
	}
	public get declined(): boolean {
		return this.currentResponse === Declined;
	}
	public get noAnswer(): boolean {
		return this.currentResponse === NoAnswer;
	}

	constructor(private server: OrganizeAppointmentClient) { }

	public accept(event: MouseEvent): void {
		this.clickedResponse(event, Accepted);
	}
	public decline(event: MouseEvent): void {
		this.clickedResponse(event, Declined);
	}
	public removeAnswer(event: MouseEvent): void {
		this.clickedResponse(event, NoAnswer);
	}

	public clickedResponse(event: MouseEvent, clicked?: AppointmentParticipationAnswer): void {
		// Prevent any elements like expansion panels to trigger
		event.stopPropagation();

		if (this.readonly) {
			return;
		}

		if (clicked !== this.currentResponse) {
			this.server.setAppointmentResponse(this.appointmentId, this.participantId, clicked).subscribe(
				() => this.selectedNewResponse.emit(clicked)
			);
		}
	}
}
