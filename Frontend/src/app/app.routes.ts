import { Routes } from '@angular/router';

import { HomeComponent } from './authenticated/home/home.component';
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

export const ROUTES: Routes = [
	{
		path: '',
		component: AuthenticatedLayoutComponent,
		canActivateChild: [ CanActivateViaAuthGuard ],
		children: [
			// TODO Use real components
			{ path: '', redirectTo: 'Events', pathMatch: 'full' },
			{ path: 'Profile', component: ProfileComponent },
			{ path: 'Events', component: HomeComponent },
			{ path: 'CreateEvent', component: CreateEventComponent },
			{ path: 'EditEvent/:id', component: EditEventComponent },
			{ path: 'ViewEvent/:id', component: HomeComponent },
			{ path: 'AcceptInvitation/:token', component: HomeComponent }
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
