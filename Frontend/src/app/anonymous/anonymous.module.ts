import { NgModule } from '@angular/core';

// Different sites
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

// Other
import { SharedModule } from '../shared/shared.module';
import { AnonymousLayoutComponent } from './anonymous-layout/anonymous-layout.component';

const components = [
	AnonymousLayoutComponent,
	LoginComponent,
	RegisterComponent,
	ResetPasswordComponent
];

@NgModule({
	declarations: [
		...components
	],
	imports: [
		SharedModule
	]
})
export class AnonymousModule {

}
