import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';
import { GeneralEventInformation } from '../../shared/server-model/general-event-information.model';

@Component({
	selector: 'edit-notifications',
	styleUrls: ['./edit-notifications.component.scss'],
	templateUrl: './edit-notifications.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditNotificationsComponent {
	public form: FormGroup;

	@Input()
	public eventInfo: GeneralEventInformation;

	private _notifications: NotificationConfiguration;
	public get notifications(): NotificationConfiguration {
		return this._notifications;
	}

	@Input()
	public set notifications(notificationConfig: NotificationConfiguration) {
		this._notifications = notificationConfig;

		this.form.reset({
			reminderCtrl: notificationConfig.sendReminderEmail,
			summaryCtrl: notificationConfig.sendSummaryEmail,
			lastMinuteCtrl: notificationConfig.sendLastMinuteChangesEmail
		});
	}

	public get formValid(): boolean {
		return this.form.valid && this.form.dirty;
	}

	public get updatedNotifications(): NotificationConfiguration {
		return {
			sendReminderEmail: this.form.get('reminderCtrl').value,
			sendSummaryEmail: this.form.get('summaryCtrl').value,
			sendLastMinuteChangesEmail: this.form.get('lastMinuteCtrl').value
		};
	}

	constructor(formBuilder: FormBuilder) {
					this.form = formBuilder.group({
						reminderCtrl: [true],
						summaryCtrl: [true],
						lastMinuteCtrl: [true]
					});
				}
}
