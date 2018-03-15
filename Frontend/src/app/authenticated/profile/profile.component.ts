import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Constants } from '../../shared/constants';
import { UserClient } from '../../shared/backend-clients/user.client';
import { AuthService } from '../../shared/services/auth.service';
import { AreYouSureDialogComponent } from '../../shared/are-you-sure-dialog/are-you-sure-dialog.component';

@Component({
	selector: 'profile',
	styleUrls: ['./profile.component.scss'],
	templateUrl: './profile.component.html'
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

	public setUserData() {
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

				this.snackBar.open('Persönliche Daten gespeichert', 'Ok');
			}
		);
	}

	public changePassword() {
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

	public async deleteProfile() {
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

	public async logOut() {
		await this.authService.logOut();
		this.snackBar.open('Erfolgreich abgemeldet', 'Ok');
		this.router.navigate(['/Login']);
	}
}
