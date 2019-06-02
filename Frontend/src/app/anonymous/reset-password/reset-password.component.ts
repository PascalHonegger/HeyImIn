import { Component, ViewChild, ChangeDetectionStrategy, } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Constants } from '../../shared/constants';
import { MatStepper } from '@angular/material';
import { ResetPasswordClient } from '../../shared/backend-clients/reset-password.client';

@Component({
	styleUrls: ['./reset-password.component.scss'],
	templateUrl: './reset-password.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordComponent {
	@ViewChild('stepper', { static: false })
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
