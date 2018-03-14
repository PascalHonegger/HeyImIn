import { Component, OnInit, Input } from '@angular/core';
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

@Component({
	selector: 'edit-general-event-info',
	styleUrls: ['./edit-general-event-info.component.scss'],
	templateUrl: './edit-general-event-info.component.html'
})
export class EditGeneralEventInfoComponent {
	public form: FormGroup;

	private _eventInfo: GeneralEventInfo;
	public get eventInfo(): GeneralEventInfo {
		return this._eventInfo;
	}

	@Input()
	public set eventInfo(info: GeneralEventInfo) {
		this._eventInfo = info;

		this.form.reset({
			titleCtrl: info.title,
			meetingPlaceCtrl: info.meetingPlace,
			descriptionCtrl: info.description,
			isPrivateCtrl: info.isPrivate,
			reminderCtrl: info.reminderTimeWindowInHours,
			summaryCtrl: info.summaryTimeWindowInHours,
		});
	}

	public get formValid(): boolean {
		return this.form.valid;
	}

	public get updatedEventInfo(): GeneralEventInfo {
		return {
			title: this.form.get('titleCtrl').value,
			meetingPlace: this.form.get('meetingPlaceCtrl').value,
			description: this.form.get('descriptionCtrl').value,
			isPrivate: this.form.get('isPrivateCtrl').value,
			reminderTimeWindowInHours: this.form.get('reminderCtrl').value,
			summaryTimeWindowInHours: this.form.get('summaryCtrl').value
		};
	}

	constructor(private eventServer: ParticipateEventClient,
				private organizeEventServer: OrganizeEventClient,
				private organizeAppointmentServer: OrganizeAppointmentClient,
				private snackBar: MatSnackBar,
				private router: Router,
				private dialog: MatDialog,
				formBuilder: FormBuilder,
				route: ActivatedRoute) {
					this.form = formBuilder.group({
						titleCtrl: ['', [Validators.required, Validators.maxLength(Constants.titleMaxLength)]],
						meetingPlaceCtrl: ['', [Validators.required, Validators.maxLength(Constants.meetingPlaceMaxLength)]],
						descriptionCtrl: ['', [Validators.required, Validators.maxLength(Constants.descriptionMaxLength)]],
						isPrivateCtrl: [false],
						reminderCtrl: [0, Validators.required, Validators.min(0)],
						summaryCtrl: [0, Validators.required, Validators.min(0)]
					});
				}
}
