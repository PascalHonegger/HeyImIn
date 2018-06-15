import { MatDialog } from '@angular/material';
import { AuthService } from '../shared/services/auth.service';
import { ParticipateEventClient } from '../shared/backend-clients/participate-event.client';
import { AreYouSureDialogComponent } from '../shared/are-you-sure-dialog/are-you-sure-dialog.component';

export abstract class DetailOverviewBase {
	public currentUserId: number;

	constructor(protected eventServer: ParticipateEventClient,
				private dialog: MatDialog,
				authService: AuthService) {
					this.currentUserId = authService.session.userId;
				}

	public async leaveEventAsync(eventId: number) {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie diesen Event und alle damit verbundenen Termine verlassen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (result) {
			await this.eventServer.removeFromEvent(eventId, this.currentUserId).toPromise();
		}
	}

	public async joinEventAsync(eventId: number) {
		await this.eventServer.joinEvent(eventId).toPromise();
	}
}
