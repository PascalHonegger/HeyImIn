import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { FormControl, Validators } from '@angular/forms';

@Component({
	styleUrls: ['./add-participant-dialog.component.scss'],
	templateUrl: './add-participant-dialog.component.html'
})
export class AddParticipantDialogComponent {
	private static readonly simpleMailPattern = '\\S+@\\S+'; // https://stackoverflow.com/a/742455
	private static readonly regexPattern = new RegExp(AddParticipantDialogComponent.simpleMailPattern, 'g');
	private static readonly multiLineRegexPattern = new RegExp(`^(${AddParticipantDialogComponent.simpleMailPattern}\\s?)+$`);

	public inputEmails: string = '';
	public emailsCtrl = new FormControl('', [Validators.required, Validators.pattern(AddParticipantDialogComponent.multiLineRegexPattern)]);

	constructor(private dialogRef: MatDialogRef<AddParticipantDialogComponent, string[]>) {
	}

	public parseAndReturnEmails() {
		const matches = this.inputEmails.match(AddParticipantDialogComponent.regexPattern);
		this.dialogRef.close(matches);
	}
}
