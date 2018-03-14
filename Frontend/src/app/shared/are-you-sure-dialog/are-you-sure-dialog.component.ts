import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
	styleUrls: ['./are-you-sure-dialog.component.scss'],
	templateUrl: './are-you-sure-dialog.component.html'
})
export class AreYouSureDialogComponent {
	constructor(@Inject(MAT_DIALOG_DATA) public body: string) {

	}
}
