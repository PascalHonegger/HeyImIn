import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Validators } from '@angular/forms';
import type { FormGroup, FormBuilder } from '@angular/forms';
import type { MatDialog } from '@angular/material/dialog';
import type { MatSnackBar } from '@angular/material/snack-bar';
import type { Router } from '@angular/router';
import type { UserClient } from '../../shared/backend-clients/user.client';
import type { AuthService } from '../../shared/services/auth.service';
import { Constants } from '../../shared/constants';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';

@Component({
	styleUrls: ['./profile.component.scss'],
	templateUrl: './profile.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent {
	public userDataForm: FormGroup;
	public passwordForm: FormGroup;

	constructor(private server: UserClient,
				private snackBar: MatSnackBar,
				private router: Router,
				private dialog: MatDialog,
				private authService: AuthService,
				formBuilder: FormBuilder) {
					this.userDataForm = formBuilder.group({
						nameCtrl: [authService.session.fullName, [Validators.required, Validators.maxLength(Constants.userFullNameMaxLength)]],
						mailCtrl: [authService.session.email, [Validators.required, Validators.email, Validators.maxLength(Constants.userEmailMaxLength)]]
					});

					this.passwordForm = formBuilder.group({
						currentPasswordCtrl: ['', [Validators.required]],
						newPasswordCtrl: ['', [Validators.required]]
					});
				}

	public setUserData(): void {
		if (!this.userDataForm.valid) {
			return;
		}

		const newName = this.userDataForm.get('nameCtrl').value;
		const newMail = this.userDataForm.get('mailCtrl').value;
		this.server.setNewUserData(newName, newMail).subscribe(
			() => {
				this.userDataForm.reset({
					nameCtrl: newName,
					mailCtrl: newMail
				});

				this.authService.session.fullName = newName;
				this.authService.session.email = newMail;

				this.snackBar.open('Persönliche Daten gespeichert', 'Ok');
			}
		);
	}

	public changePassword(): void {
		if (!this.passwordForm.valid) {
			return;
		}

		this.server.setNewPassword(this.passwordForm.get('currentPasswordCtrl').value, this.passwordForm.get('newPasswordCtrl').value).subscribe(
			() => {
				this.passwordForm.reset({
					currentPasswordCtrl: '',
					newPasswordCtrl: ''
				});
				this.snackBar.open('Passwort geändert', 'Ok');
			}
		);
	}

	public async deleteProfile(): Promise<void> {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie wirklich Ihr Profil und alle damit verbundenen Daten löschen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (result) {
			this.server.deleteAccount().subscribe(() => {
				this.authService.clearLocalSession();
				this.snackBar.open('Profil gelöscht', 'Ok');
				this.router.navigate(['/Login']);
			});
		}
	}

	public async logOut(): Promise<void> {
		await this.authService.logOut();
		this.snackBar.open('Erfolgreich abgemeldet', 'Ok');
		this.router.navigate(['/Login']);
	}
}
