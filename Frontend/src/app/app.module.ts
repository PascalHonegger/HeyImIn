import { FlexLayoutModule } from '@angular/flex-layout';
import { BrowserModule, Title } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule, LOCALE_ID } from '@angular/core';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

// Import swiss german
import { registerLocaleData } from '@angular/common';
import localeDeCh from '@angular/common/locales/de-ch';

registerLocaleData(localeDeCh);

/*
 * Platform and Environment providers/directives/pipes
 */
import { environment } from 'environments/environment';
import { ROUTES } from './app.routes';

// App is our top level component
import { AppComponent } from './app.component';

// Main layout
import { NavigationToolbarComponent } from './shared/navigation-toolbar/navigation-toolbar.component';
import { AuthenticatedLayoutComponent } from './authenticated/authenticated-layout/authenticated-layout.component';
import { AnonymousLayoutComponent } from './anonymous/anonymous-layout/anonymous-layout.component';

// Different sites
import { AcceptInviteComponent } from './authenticated/accept-invite/accept-invite.component';
import { LoginComponent } from './anonymous/login/login.component';
import { RegisterComponent } from './anonymous/register/register.component';
import { ResetPasswordComponent } from './anonymous/reset-password/reset-password.component';
import { ProfileComponent } from './authenticated/profile/profile.component';
import { EventsOverviewComponent } from './authenticated/events-overview/events-overview.component';
import { CreateEventComponent } from './authenticated/create-event/create-event.component';
import { EditEventComponent } from './authenticated/edit-event/edit-event.component';
import { ViewEventComponent } from './authenticated/view-event/view-event.component';

// Components used by the different sites
import { EditGeneralEventInfoComponent } from './authenticated/edit-general-event-details/edit-general-event-info.component';
import { EventParticipantTableComponent } from './authenticated/event-participant-table/event-participant-table.component';
import { EditNotificationsComponent } from './authenticated/edit-notifications/edit-notifications.component';
import { EventParticipationComponent } from './authenticated/event-participation/event-participation.component';
import { EventsOverviewListComponent } from './authenticated/events-overview-list/events-overview-list.component';
import { EventInfoDisplayComponent } from './authenticated/event-info-display/event-info-display.component';

// Dialog contents
import { ErrorDialogComponent } from './shared/error-dialog/error-dialog.component';
import { LoadingDialogComponent } from './shared/loading-dialog/loading-dialog.component';
import { AreYouSureDialogComponent } from './shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { AddAppointmentsDialogComponent } from './authenticated/add-appointments-dialog/add-appointments-dialog.component';

// 404 not found page
import { NoContentComponent } from './shared/no-content/no-content.component';

// Backend clients
import { OrganizeEventClient } from './shared/backend-clients/organize-event.client';
import { ParticipateEventClient } from './shared/backend-clients/participate-event.client';
import { OrganizeAppointmentClient } from './shared/backend-clients/organize-appointment.client';
import { InviteToEventClient } from './shared/backend-clients/invite-to-event.client';
import { UserClient } from './shared/backend-clients/user.client';
import { SessionClient } from './shared/backend-clients/session.client';
import { ResetPasswordClient } from './shared/backend-clients/reset-password.client';

// Services
import { AuthService } from './shared/services/auth.service';

// Interceptors
import { AppendSessionTokenInterceptor } from './shared/interceptors/append-session-token.interceptor';
import { ErrorHandlerInterceptor } from './shared/interceptors/error-handler.interceptor';
import { ShowLoadingDialogInterceptor } from './shared/interceptors/show-loading-dialog.interceptor';

// Guards
import { CanActivateViaAuthGuard } from './shared/guards/can-activate-via-auth.guard';

// Material 2
import {
	MatButtonModule,
	MatToolbarModule,
	MatDialogModule,
	MatTooltipModule,
	MatInputModule,
	MatSnackBarModule,
	MatCardModule,
	MatIconModule,
	MatTableModule,
	MatProgressSpinnerModule,
	MatStepperModule,
	MatSlideToggleModule,
	MatExpansionModule,
	MatSortModule
} from '@angular/material';

// Import global styles & theme
import '../styles/styles.scss';
/**
 * `AppModule` is the main entry point into Angular2's bootstraping process
 */
@NgModule({
	bootstrap: [ AppComponent ],
	declarations: [
		// Shared
		AppComponent,
		NavigationToolbarComponent,
		ErrorDialogComponent,
		LoadingDialogComponent,
		AreYouSureDialogComponent,
		NoContentComponent,
		// Anonymous
		AnonymousLayoutComponent,
		LoginComponent,
		RegisterComponent,
		ResetPasswordComponent,
		// Authenticated
		AuthenticatedLayoutComponent,
		AcceptInviteComponent,
		EventsOverviewComponent,
		ProfileComponent,
		CreateEventComponent,
		EditEventComponent,
		ViewEventComponent,
		EditGeneralEventInfoComponent,
		EventParticipantTableComponent,
		AddAppointmentsDialogComponent,
		EditNotificationsComponent,
		EventParticipationComponent,
		EventsOverviewListComponent,
		EventInfoDisplayComponent
	],
	// Dialog contents have to be specified here
	entryComponents: [
		ErrorDialogComponent,
		LoadingDialogComponent,
		AreYouSureDialogComponent,
		AddAppointmentsDialogComponent
	],
	/**
	 * Import Angular's modules.
	 */
	imports: [
		// Angular
		BrowserModule,
		BrowserAnimationsModule,
		FormsModule,
		ReactiveFormsModule,
		HttpClientModule,
		RouterModule.forRoot(ROUTES, {
			useHash: true,
			preloadingStrategy: PreloadAllModules
		}),
		// Angular Material
		MatInputModule,
		MatButtonModule,
		MatToolbarModule,
		MatDialogModule,
		MatTooltipModule,
		MatSnackBarModule,
		MatCardModule,
		MatIconModule,
		MatProgressSpinnerModule,
		MatStepperModule,
		MatSlideToggleModule,
		MatTableModule,
		MatExpansionModule,
		MatSortModule,
		// Flex layout
		FlexLayoutModule
	],
	/**
	 * Expose our Services and Providers into Angular's dependency injection.
	 */
	providers: [
		environment.ENV_PROVIDERS,
		// Guards
		CanActivateViaAuthGuard,
		// Services
		AuthService,
		// Backend clients
		InviteToEventClient,
		OrganizeAppointmentClient,
		OrganizeEventClient,
		ParticipateEventClient,
		SessionClient,
		ResetPasswordClient,
		UserClient,
		// Ensure 3rd party components use english
		{
			provide: LOCALE_ID,
			useValue: 'de-CH'
		},
		// Hook up HTTP interceptors
		{
			provide: HTTP_INTERCEPTORS,
			useClass: AppendSessionTokenInterceptor,
			multi: true
		},
		{
			provide: HTTP_INTERCEPTORS,
			useClass: ShowLoadingDialogInterceptor,
			multi: true
		},
		{
			provide: HTTP_INTERCEPTORS,
			useClass: ErrorHandlerInterceptor,
			multi: true
		}
	]
})
export class AppModule {

}
