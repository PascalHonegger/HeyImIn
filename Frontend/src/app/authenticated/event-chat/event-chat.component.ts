import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import type { OnInit } from '@angular/core';
import type { EventChatMessage } from '../../shared/server-model/event-chat-message.model';
import type { EventChatClient } from '../../shared/backend-clients/event-chat.client';
import type { UserInformation } from '../../shared/server-model/user-information.model';
import type { AuthService } from '../../shared/services/auth.service';
import { Constants } from '../../shared/constants';

@Component({
	selector: 'event-chat',
	templateUrl: './event-chat.component.html',
	styleUrls: ['./event-chat.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventChatComponent implements OnInit {
	@Input()
	public eventId: number;

	public chatMessages = new BehaviorSubject<readonly EventChatMessage[]>([]);
	public hasMoreMessages = new BehaviorSubject(false);
	public chatMessageCtrl = new FormControl('', [Validators.maxLength(Constants.chatMessageMaxLength)]);
	public userList$ = new BehaviorSubject<UserInformation[]>([]);

	constructor(private server: EventChatClient, authService: AuthService) {
		this.userList$.next([{
			userId: authService.session.userId,
			name: authService.session.fullName,
			email: authService.session.email
		}]);
	}

	public ngOnInit(): void {
		this.reload();
	}

	public getChatMessageId(_index: number, chatMessage: EventChatMessage) {
		return chatMessage.id;
	}

	public reload() {
		this.server.getChatMessages(this.eventId).subscribe(response => {
			this.chatMessages.next(response.messages);
			this.hasMoreMessages.next(response.possiblyMoreMessages);
			this.updateAuthorInformation(response.authorInformations);
		});
	}

	public getUserData$(userId: number): Observable<UserInformation> {
		return this.userList$.pipe(map(l => l.find(u => u.userId === userId)));
	}

	public loadPreviousChatMessages() {
		this.hasMoreMessages.next(false);
		const messagesArray = this.chatMessages.value;
		const earliestLoadedMessage = messagesArray[messagesArray.length - 1];

		this.server.getChatMessages(this.eventId, earliestLoadedMessage).subscribe(response => {
			this.chatMessages.next(messagesArray.concat(response.messages));
			this.hasMoreMessages.next(response.possiblyMoreMessages);
			this.updateAuthorInformation(response.authorInformations);
		}, _err => {
			this.hasMoreMessages.next(true);
		});
	}

	public sendChatMessage() {
		const messageContent = this.chatMessageCtrl.value;
		this.chatMessageCtrl.reset('');
		this.server.sendChatMessage(this.eventId, messageContent).subscribe(m => {
			this.chatMessages.next([m].concat(this.chatMessages.value));
		}, _err => {
			this.chatMessageCtrl.setValue(messageContent);
		});
	}

	private updateAuthorInformation(authorInformation: readonly UserInformation[]) {
		const currentList = this.userList$.value;
		const newInformation = authorInformation.filter((author, index) =>
			authorInformation.indexOf(author) === index && currentList.every(u => u.userId !== author.userId));
		if (newInformation.length > 0) {
			this.userList$.next(currentList.concat(newInformation));
		}
	}
}
