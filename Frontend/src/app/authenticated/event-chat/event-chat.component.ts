import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { EventChatMessage } from '../../shared/server-model/event-chat-message.model';
import { EventChatClient } from '../../shared/backend-clients/event-chat.client';
import { Constants } from '../../shared/constants';
import { BehaviorSubject } from 'rxjs';

@Component({
	selector: 'event-chat',
	templateUrl: './event-chat.component.html',
	styleUrls: ['./event-chat.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventChatComponent implements OnInit {
	@Input()
	public eventId: number;

	public chatMessages = new BehaviorSubject<ReadonlyArray<EventChatMessage>>([]);
	public hasMoreMessages = new BehaviorSubject(false);
	public chatMessageCtrl = new FormControl('', [Validators.required, Validators.maxLength(Constants.chatMessageMaxLength)]);

	constructor(private server: EventChatClient) { }

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
		});
	}

	public loadPreviousChatMessages() {
		this.hasMoreMessages.next(false);
		const messagesArray = this.chatMessages.value;
		const earliestLoadedMessage = messagesArray[messagesArray.length - 1];

		this.server.getChatMessages(this.eventId, earliestLoadedMessage).subscribe(response => {
			this.chatMessages.next(messagesArray.concat(response.messages));
			this.hasMoreMessages.next(response.possiblyMoreMessages);
		}, err => {
			this.hasMoreMessages.next(true);
		});
	}

	public sendChatMessage() {
		const messageContent = this.chatMessageCtrl.value;
		this.chatMessageCtrl.reset('');
		this.server.sendChatMessage(this.eventId, messageContent).subscribe(m => {
			this.chatMessages.next([m].concat(this.chatMessages.value));
		}, err => {
			this.chatMessageCtrl.setValue(messageContent);
		});
	}
}
