import { Routes } from '@angular/router';

import { AcceptInviteComponent } from './authenticated/accept-invite/accept-invite.component';
import { NoContentComponent } from './shared/no-content/no-content.component';
import { CanActivateViaAuthGuard } from './shared/guards/can-activate-via-auth.guard';
import { LoginComponent } from './anonymous/login/login.component';
import { AuthenticatedLayoutComponent } from './authenticated/authenticated-layout/authenticated-layout.component';
import { AnonymousLayoutComponent } from './anonymous/anonymous-layout/anonymous-layout.component';
import { RegisterComponent } from './anonymous/register/register.component';
import { ResetPasswordComponent } from './anonymous/reset-password/reset-password.component';
import { ProfileComponent } from './authenticated/profile/profile.component';
import { EditEventComponent } from './authenticated/edit-event/edit-event.component';
import { CreateEventComponent } from './authenticated/create-event/create-event.component';
import { ViewEventComponent } from './authenticated/view-event/view-event.component';
import { EventsOverviewComponent } from './authenticated/events-overview/events-overview.component';

export const ROUTES: Routes = [
	{
		path: '',
		component: AuthenticatedLayoutComponent,
		canActivateChild: [ CanActivateViaAuthGuard ],
		children: [
			{ path: '', redirectTo: 'Events', pathMatch: 'full' },
			{ path: 'Profile', component: ProfileComponent },
			{ path: 'Events', component: EventsOverviewComponent },
			{ path: 'CreateEvent', component: CreateEventComponent },
			{ path: 'EditEvent/:id', component: EditEventComponent },
			{ path: 'ViewEvent/:id', component: ViewEventComponent },
			{ path: 'AcceptInvitation/:token', component: AcceptInviteComponent }
		]
	},
	{
		path: '',
		component: AnonymousLayoutComponent,
		children: [
			{ path: 'Login', component: LoginComponent },
			{ path: 'Register', component: RegisterComponent },
			{ path: 'ResetPassword', component: ResetPasswordComponent }
		]
	},
	{ path: '**', component: NoContentComponent }
];
