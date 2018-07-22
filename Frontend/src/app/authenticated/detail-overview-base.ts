import { MatDialog } from '@angular/material';
import { AuthService } from '../shared/services/auth.service';
import { ParticipateEventClient } from '../shared/backend-clients/participate-event.client';
import { AreYouSureDialogComponent } from '../shared/are-you-sure-dialog/are-you-sure-dialog.component';
import { FrontendSession } from '../shared/server-model/frontend-session.model';
import { UserInformation } from '../shared/server-model/user-information.model';
import { AppointmentDetails } from '../shared/server-model/appointment-details.model';
import { AppointmentParticipationAnswer, NoAnswer } from '../shared/server-model/appointment-participation-answer.model';

export abstract class DetailOverviewBase {
	public readonly currentSession: FrontendSession;
	protected currentUserInformation: UserInformation;

	constructor(protected eventServer: ParticipateEventClient,
				private dialog: MatDialog,
				authService: AuthService) {
					this.currentSession = authService.session;
					this.currentUserInformation = {
						userId: this.currentSession.userId,
						name: this.currentSession.fullName,
						email: this.currentSession.email
					};
				}

	public async leaveEventAsync(eventId: number) {
		const result = await this.dialog
			.open(AreYouSureDialogComponent, {
				data: 'Möchten Sie diesen Event und alle damit verbundenen Termine verlassen?',
				closeOnNavigation: true
			}).afterClosed().toPromise();

		if (result) {
			await this.eventServer.removeFromEvent(eventId, this.currentSession.userId).toPromise();
		}
	}

	public async joinEventAsync(eventId: number) {
		await this.eventServer.joinEvent(eventId).toPromise();
	}

	public getCurrentResponse(appointment: AppointmentDetails): AppointmentParticipationAnswer | undefined {
		const givenAnswer = appointment.participations.find(p => p.participantId === this.currentSession.userId);
		if (givenAnswer != null) {
			return givenAnswer.response;
		}

		return NoAnswer;
	}
}
