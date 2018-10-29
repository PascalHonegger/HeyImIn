import { Component, ChangeDetectionStrategy, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';
import { UserInformation } from '../../shared/server-model/user-information.model';

@Component({
	styleUrls: ['./change-organizer-dialog.component.scss'],
	templateUrl: './change-organizer-dialog.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangeOrganizerDialogComponent {
	constructor(@Inject(MAT_DIALOG_DATA) public parameter: ChangeOrganizerDialogParameter) {
	}
}

export interface ChangeOrganizerDialogParameter {
	participants: ReadonlyArray<UserInformation>;
}
