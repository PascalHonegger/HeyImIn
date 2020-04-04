import { Component, ViewChild, ChangeDetectionStrategy, } from '@angular/core';
import { Validators } from '@angular/forms';
import type { FormGroup, FormBuilder } from '@angular/forms';
import type { MatStepper } from '@angular/material/stepper';
import type { ResetPasswordClient } from '../../shared/backend-clients/reset-password.client';
import { Constants } from '../../shared/constants';

@Component({
	styleUrls: ['./reset-password.component.scss'],
	templateUrl: './reset-password.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordComponent {
	@ViewChild('stepper')
	public stepper: MatStepper;

	public requestTokenForm: FormGroup;
	public setNewPasswordForm: FormGroup;

	constructor(private server: ResetPasswordClient, formBuilder: FormBuilder) {
		this.requestTokenForm = formBuilder.group({
			mailCtrl: ['', [Validators.required, Validators.email, Validators.maxLength(Constants.userEmailMaxLength)]],
		});

		this.setNewPasswordForm = formBuilder.group({
			codeCtrl: ['', [Validators.required, Validators.pattern(Constants.guidRegex)]],
			passwordCtrl: ['', Validators.required]
		});
	}

	public requestResetCode() {
		this.server.requestPasswordReset(this.requestTokenForm.get('mailCtrl').value).subscribe(
			() => this.stepper.next()
		);
	}

	public resetPassword() {
		this.server.resetPassword(this.setNewPasswordForm.get('codeCtrl').value, this.setNewPasswordForm.get('passwordCtrl').value).subscribe(
			() => this.stepper.next()
		);
	}
}
