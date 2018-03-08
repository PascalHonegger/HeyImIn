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
	MatStepperModule
} from '@angular/material';

// Import global styles & theme
import '../styles/styles.scss';

/**
 * `AppModule` is the main entry point into Angular2's bootstraping process
 */
@NgModule({
	bootstrap: [ AppComponent ],
	declarations: [
		AppComponent
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
		MatTableModule,
		// Flex layout
		FlexLayoutModule
	],
	/**
	 * Expose our Services and Providers into Angular's dependency injection.
	 */
	providers: [
		environment.ENV_PROVIDERS,
		{
			provide: LOCALE_ID,
			useValue: 'de-CH'
		}
	]
})
export class AppModule {

}
