import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { startOfHour, addDays, format, parse } from 'date-fns';
import { FormControl, Validators } from '@angular/forms';

@Component({
	styleUrls: ['./add-appointments-dialog.component.scss'],
	templateUrl: './add-appointments-dialog.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddAppointmentsDialogComponent {
	private static readonly datePattern = '\\d{2}\\.\\d{2}\\.\\d{4} \\d{2}\\:\\d{2}';
	private static readonly regexPattern = new RegExp(AddAppointmentsDialogComponent.datePattern, 'g');
	private static readonly multiLineRegexPattern = new RegExp(`^(${AddAppointmentsDialogComponent.datePattern}\\s?)+$`);

	private static readonly dateFormat = 'dd.MM.yyyy HH:mm';

	public inputDates = '';
	public exampleDate: string;
	public datesCtrl = new FormControl('', [Validators.required, Validators.pattern(AddAppointmentsDialogComponent.multiLineRegexPattern)]);

	constructor(private dialogRef: MatDialogRef<AddAppointmentsDialogComponent, Date[]>) {
		const tomorrow = addDays(new Date(), 1);
		const withNiceTime = startOfHour(tomorrow);
		this.exampleDate = format(withNiceTime, AddAppointmentsDialogComponent.dateFormat);
	}

	public parseAndReturnDates() {
		const matches = this.inputDates.match(AddAppointmentsDialogComponent.regexPattern);
		const parsedDates = matches.map(match => parse(match, AddAppointmentsDialogComponent.dateFormat, new Date()));
		this.dialogRef.close(parsedDates);
	}
}
