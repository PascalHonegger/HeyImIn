import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar, MatDialog } from '@angular/material';
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
import { EditEventDetails } from '../../shared/server-model/event-edit-details.model';
import { GeneralEventInfo } from '../../shared/server-model/general-event-info.model';
import { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';
import { AppointmentParticipationAnswer } from '../../shared/server-model/appointment-participation-answer.model';

@Component({
	selector: 'appointment-participation',
	styleUrls: ['./appointment-participation.component.scss'],
	templateUrl: './appointment-participation.component.html'
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
