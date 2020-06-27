import { NgModule, LOCALE_ID } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { SharedModule } from './shared/shared.module';
import { AuthenticatedModule } from './authenticated/authenticated.module';
import { AnonymousModule } from './anonymous/anonymous.module';
import { AppComponent } from './app.component';

// Ensure swiss german language for angular pipes
import '@angular/common/locales/global/de-CH';

import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';

@NgModule({
	declarations: [
		AppComponent
	],
	imports: [
		SharedModule.forRoot(),
		AuthenticatedModule,
		AnonymousModule,
		AppRoutingModule,
		ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production })
	],
	providers: [ { provide: LOCALE_ID, useValue: 'de-CH' } ],
	bootstrap: [AppComponent]
})
export class AppModule { }
