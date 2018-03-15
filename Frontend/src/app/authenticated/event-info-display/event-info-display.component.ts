import { Component, Input } from '@angular/core';
import { ViewEventInformation } from '../../shared/server-model/view-event-information.model';

@Component({
	selector: 'event-info-display',
	styleUrls: ['./event-info-display.component.scss'],
	templateUrl: './event-info-display.component.html'
})
export class EventInfoDisplayComponent {
	@Input()
	public info: ViewEventInformation;
}
