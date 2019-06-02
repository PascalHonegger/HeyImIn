import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { FormControl, Validators } from '@angular/forms';

@Component({
	styleUrls: ['./add-participant-dialog.component.scss'],
	templateUrl: './add-participant-dialog.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddParticipantDialogComponent {
	private static readonly simpleMailPattern = '\\S+@\\S+'; // https://stackoverflow.com/a/742455
	private static readonly regexPattern = new RegExp(AddParticipantDialogComponent.simpleMailPattern, 'g');
	private static readonly multiLineRegexPattern = new RegExp(`^(${AddParticipantDialogComponent.simpleMailPattern}\\s?)+$`);

	public readonly emailsCtrl = new FormControl('',
		[Validators.required, Validators.pattern(AddParticipantDialogComponent.multiLineRegexPattern)]);

	constructor(private dialogRef: MatDialogRef<AddParticipantDialogComponent, string[]>) {
	}

	public parseAndReturnEmails() {
		const inputEmails: string = this.emailsCtrl.value;
		const matches = inputEmails.match(AddParticipantDialogComponent.regexPattern) || [];
		this.dialogRef.close(matches);
	}
}
