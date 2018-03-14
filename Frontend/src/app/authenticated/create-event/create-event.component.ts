import { Component, OnInit } from '@angular/core';
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
	selector: 'create-event',
	styleUrls: ['./create-event.component.scss'],
	templateUrl: './create-event.component.html'
})
export class CreateEventComponent {
	public defaultEventInfo: GeneralEventInfo = {
		title: '',
		meetingPlace: '',
		description: '',
		isPrivate: false,
		reminderTimeWindowInHours: 48,
		summaryTimeWindowInHours: 4
	};

	constructor(private organizeEventServer: OrganizeEventClient,
				private router: Router) { }

	public createEvent(newEventInfo: GeneralEventInfo) {
		this.organizeEventServer
			.createEvent(newEventInfo)
			.subscribe((id) => this.router.navigate(['/ViewEvent', id]));
	}
}
