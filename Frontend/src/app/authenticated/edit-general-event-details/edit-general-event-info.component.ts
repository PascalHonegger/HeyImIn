import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { Validators } from '@angular/forms';
import type { FormGroup, FormBuilder } from '@angular/forms';
import type { GeneralEventInformation } from '../../shared/server-model/general-event-information.model';
import { Constants } from '../../shared/constants';

@Component({
	selector: 'edit-general-event-info',
	styleUrls: ['./edit-general-event-info.component.scss'],
	templateUrl: './edit-general-event-info.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditGeneralEventInfoComponent {
	public form: FormGroup;

	private _eventInfo: GeneralEventInformation;
	public get eventInfo(): GeneralEventInformation {
		return this._eventInfo;
	}

	@Input()
	public set eventInfo(info: GeneralEventInformation) {
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
		return this.form.valid && this.form.dirty;
	}

	public get updatedEventInfo(): GeneralEventInformation {
		return {
			title: this.form.get('titleCtrl').value,
			meetingPlace: this.form.get('meetingPlaceCtrl').value,
			description: this.form.get('descriptionCtrl').value,
			isPrivate: this.form.get('isPrivateCtrl').value,
			reminderTimeWindowInHours: this.form.get('reminderCtrl').value,
			summaryTimeWindowInHours: this.form.get('summaryCtrl').value
		};
	}

	constructor(formBuilder: FormBuilder) {
					this.form = formBuilder.group({
						titleCtrl: ['', [Validators.required, Validators.maxLength(Constants.titleMaxLength)]],
						meetingPlaceCtrl: ['', [Validators.required, Validators.maxLength(Constants.meetingPlaceMaxLength)]],
						descriptionCtrl: ['', [Validators.required, Validators.maxLength(Constants.descriptionMaxLength)]],
						isPrivateCtrl: [false],
						reminderCtrl: [0,
							[Validators.required, Validators.min(Constants.realisticMinimumHours), Validators.max(Constants.realisticMaximumHours)]],
						summaryCtrl: [0,
							[Validators.required, Validators.min(Constants.realisticMinimumHours), Validators.max(Constants.realisticMaximumHours)]]
					});
				}
}
