import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import * as moment from 'moment';
import { FormControl, Validators } from '@angular/forms';

@Component({
	styleUrls: ['./add-appointments-dialog.component.scss'],
	templateUrl: './add-appointments-dialog.component.html'
})
export class AddAppointmentsDialogComponent {
	private static readonly datePattern = '\\d{2}\\.\\d{2}\\.\\d{4} \\d{2}\\:\\d{2}';
	private static readonly regexPattern = new RegExp(AddAppointmentsDialogComponent.datePattern, 'g');
	private static readonly multiLineRegexPattern = new RegExp(`^(${AddAppointmentsDialogComponent.datePattern}\\s?)+$`);

	private static readonly dateFormat = 'DD.MM.YYYY HH:mm';

	public inputDates = '';
	public exampleDate: string;
	public datesCtrl = new FormControl('', [Validators.required, Validators.pattern(AddAppointmentsDialogComponent.multiLineRegexPattern)]);

	constructor(private dialogRef: MatDialogRef<AddAppointmentsDialogComponent, Date[]>) {
		this.exampleDate = moment().add(1, 'days').add(1, 'hour').startOf('hour').format(AddAppointmentsDialogComponent.dateFormat);
	}

	public parseAndReturnDates() {
		const matches = this.inputDates.match(AddAppointmentsDialogComponent.regexPattern);
		const parsed = matches.map(match => moment(match, AddAppointmentsDialogComponent.dateFormat));
		const asDateObjects = parsed.map(m => m.toDate());
		this.dialogRef.close(asDateObjects);
	}
}
