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
import { ViewEventInformation } from '../../shared/server-model/view-event-information.model';

@Component({
	selector: 'event-info-display',
	styleUrls: ['./event-info-display.component.scss'],
	templateUrl: './event-info-display.component.html'
})
export class EventInfoDisplayComponent {
	@Input()
	public info: ViewEventInformation;
}
