import { NgModule, LOCALE_ID } from '@angular/core';
import { RouterModule, PreloadAllModules } from '@angular/router';

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

// Our modules
import { SharedModule } from './shared/shared.module';
import { AuthenticatedModule } from './authenticated/authenticated.module';
import { AnonymousModule } from './anonymous/anonymous.module';

// Import global styles & theme
import '../styles/styles.scss';

/**
 * `AppModule` is the main entry point into Angular2's bootstraping process
 */
@NgModule({
	bootstrap: [ AppComponent ],
	declarations: [ AppComponent ],
	imports: [
		SharedModule.forRoot(),
		AuthenticatedModule,
		AnonymousModule,
		RouterModule.forRoot(ROUTES, {
			useHash: true,
			preloadingStrategy: PreloadAllModules
		})
	],
	/**
	 * Expose our Services and Providers into Angular's dependency injection.
	 */
	providers: [
		environment.ENV_PROVIDERS,
		// Ensure 3rd party components use swiss german (E.g. date format)
		{
			provide: LOCALE_ID,
			useValue: 'de-CH'
		}
	]
})
export class AppModule {

}
