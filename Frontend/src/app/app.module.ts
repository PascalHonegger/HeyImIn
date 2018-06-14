import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { SharedModule } from './shared/shared.module';
import { AuthenticatedModule } from './authenticated/authenticated.module';
import { AnonymousModule } from './anonymous/anonymous.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

// Ensure swiss german language for angular pipes
import { registerLocaleData } from '@angular/common';
import localeDeCh from '@angular/common/locales/de-ch';
registerLocaleData(localeDeCh);

// Ensure swiss german language for moment / dates
import * as moment from 'moment';
import 'moment/locale/de-ch';

moment.locale('de-ch');

@NgModule({
	declarations: [
		AppComponent
	],
	imports: [
		SharedModule.forRoot(),
		AuthenticatedModule,
		AnonymousModule,
		BrowserModule,
		AppRoutingModule,
		BrowserAnimationsModule
	],
	providers: [ { provide: LOCALE_ID, useValue: 'de-CH' } ],
	bootstrap: [AppComponent]
})
export class AppModule { }
