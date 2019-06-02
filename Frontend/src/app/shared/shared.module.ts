import { NgModule, ModuleWithProviders } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Material 2
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldDefaultOptions } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { FlexLayoutModule } from '@angular/flex-layout';

// Interceptors
import { AppendSessionTokenInterceptor } from './interceptors/append-session-token.interceptor';
import { ShowLoadingDialogInterceptor } from './interceptors/show-loading-dialog.interceptor';
import { ErrorHandlerInterceptor } from './interceptors/error-handler.interceptor';
import { AppendApiVersionInterceptor } from './interceptors/append-api-version.interceptor';

// Dialogs
import { ErrorDialogComponent } from './error-dialog/error-dialog.component';
import { LoadingDialogComponent } from './loading-dialog/loading-dialog.component';
import { AreYouSureDialogComponent } from './are-you-sure-dialog/are-you-sure-dialog.component';

// Other
import { MainLayoutComponent } from './main-layout/main-layout.component';
import { MainTitleComponent } from './main-title/main-title.component';
import { NoContentComponent } from './no-content/no-content.component';

const dialogs = [
	ErrorDialogComponent,
	LoadingDialogComponent,
	AreYouSureDialogComponent
];

const modules = [
		// Angular
		BrowserModule,
		BrowserAnimationsModule,
		FormsModule,
		ReactiveFormsModule,
		HttpClientModule,
		RouterModule,
		// Angular Material
		MatInputModule,
		MatButtonModule,
		MatToolbarModule,
		MatDialogModule,
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
];

const components = [
	MainLayoutComponent,
	MainTitleComponent,
	NoContentComponent
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
		...modules
	],
	exports: [
		...modules,
		...components
	]
})
export class SharedModule {
	// Ensures a single instance for all services is returned
	public static forRoot(): ModuleWithProviders {
		return {
			ngModule: SharedModule,
			providers: [
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
				},
				{
					provide: HTTP_INTERCEPTORS,
					useClass: AppendApiVersionInterceptor,
					multi: true
				},
				{
					provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
					useValue: { appearance: 'outline' } as MatFormFieldDefaultOptions
				}
			]
		};
	}
}
