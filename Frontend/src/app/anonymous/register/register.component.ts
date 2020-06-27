import { Component, ChangeDetectionStrategy } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Validators } from '@angular/forms';
import type { FormGroup, FormBuilder } from '@angular/forms';
import type { Router } from '@angular/router';
import type { AuthService } from '../../shared/services/auth.service';
import type { UserClient } from '../../shared/backend-clients/user.client';
import { Constants } from '../../shared/constants';

@Component({
	styleUrls: ['./register.component.scss'],
	templateUrl: './register.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
	public form: FormGroup;

	constructor(private authService: AuthService,
				private userServer: UserClient,
				private router: Router,
				formBuilder: FormBuilder) {
					this.form = formBuilder.group({
						nameCtrl: ['', [Validators.required, Validators.maxLength(Constants.userFullNameMaxLength)]],
						mailCtrl: ['', [Validators.required, Validators.email, Validators.maxLength(Constants.userEmailMaxLength)]],
						passwordCtrl: ['', Validators.required]
					});
				}

	public async register(): Promise<void> {
		if (!this.form.valid) {
			return;
		}

		try {
			const newUserSession = await this.userServer.register(
				this.form.get('nameCtrl').value, this.form.get('mailCtrl').value, this.form.get('passwordCtrl').value).toPromise();
			this.authService.session = newUserSession;
			this.router.navigate([this.authService.urlAfterLogin]);
		} catch (err) {
			if (err instanceof HttpErrorResponse && err.status !== 400) {
				console.error('Unexpected error while registering user', err);
			}
		}
	}
}
