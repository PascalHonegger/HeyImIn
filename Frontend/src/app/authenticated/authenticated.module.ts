import { NgModule } from '@angular/core';

// Dialogs
import { AddAppointmentsDialogComponent } from './add-appointments-dialog/add-appointments-dialog.component';
import { AddParticipantDialogComponent } from './add-participant-dialog/add-participant-dialog.component';

// Profile
import { ProfileComponent } from './profile/profile.component';

// Event components
import { AcceptInviteComponent } from './accept-invite/accept-invite.component';
import { EventsOverviewComponent } from './events-overview/events-overview.component';
import { CreateEventComponent } from './create-event/create-event.component';
import { EditEventComponent } from './edit-event/edit-event.component';
import { ViewEventComponent } from './view-event/view-event.component';
import { EditGeneralEventInfoComponent } from './edit-general-event-details/edit-general-event-info.component';
import { AppointmentParticipantTableComponent } from './appointment-participant-table/appointment-participant-table.component';
import { EditNotificationsComponent } from './edit-notifications/edit-notifications.component';
import { AppointmentParticipationComponent } from './appointment-participation/appointment-participation.component';
import { EventsOverviewListComponent } from './events-overview-list/events-overview-list.component';
import { EventInfoDisplayComponent } from './event-info-display/event-info-display.component';
import { ManageEventParticipantsTableComponent } from './manage-event-participants-table/manage-event-participants-table.component';
import { AppointmentParticipationSummaryComponent } from './appointment-participation-summary/appointment-participation-summary.component';

// Other
import { AuthenticatedLayoutComponent } from './authenticated-layout/authenticated-layout.component';
import { SharedModule } from '../shared/shared.module';

const dialogs = [
	AddAppointmentsDialogComponent,
	AddParticipantDialogComponent
];

const components = [
	AuthenticatedLayoutComponent,
	AcceptInviteComponent,
	EventsOverviewComponent,
	ProfileComponent,
	CreateEventComponent,
	EditEventComponent,
	ViewEventComponent,
	EditGeneralEventInfoComponent,
	AppointmentParticipantTableComponent,
	AddAppointmentsDialogComponent,
	AddParticipantDialogComponent,
	EditNotificationsComponent,
	AppointmentParticipationComponent,
	EventsOverviewListComponent,
	EventInfoDisplayComponent,
	ManageEventParticipantsTableComponent,
	AppointmentParticipationSummaryComponent
];

@NgModule({
	declarations: [
		...components,
		...dialogs
	],
	// Dialog contents have to be specified here
	entryComponents: [
		...dialogs
	],
	imports: [
		SharedModule
	]
})
export class AuthenticatedModule {

}
