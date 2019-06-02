import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Constants } from '../../shared/constants';

@Component({
	styleUrls: ['./login.component.scss'],
	templateUrl: './login.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
	public form: FormGroup;

	constructor(private authService: AuthService,
				private snackBar: MatSnackBar,
				private router: Router,
				formBuilder: FormBuilder) {
					this.form = formBuilder.group({
						mailCtrl: ['', [Validators.required, Validators.email, Validators.maxLength(Constants.userEmailMaxLength)]],
						passwordCtrl: ['', Validators.required]
					});
				}

	public async login() {
		if (!this.form.valid) {
			return;
		}

		try {
			await this.authService.tryCreateSession(this.form.get('mailCtrl').value, this.form.get('passwordCtrl').value);
			this.router.navigate([this.authService.urlAfterLogin]);
		} catch (err) {
			if (err instanceof HttpErrorResponse && err.status === 401) {
				this.snackBar.open('Ungültige Anmeldedaten', 'Ok');
			} else {
				console.error('Unexpected error while logging in', err);
			}
		}
	}
}
