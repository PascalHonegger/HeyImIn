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
import { NotificationConfiguration } from '../../shared/server-model/notification-configuration.model';

@Component({
	selector: 'edit-notifications',
	styleUrls: ['./edit-notifications.component.scss'],
	templateUrl: './edit-notifications.component.html'
})
export class EditNotificationsComponent {
	public form: FormGroup;

	@Input()
	public eventInfo: GeneralEventInfo;

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
		return this.form.valid;
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
