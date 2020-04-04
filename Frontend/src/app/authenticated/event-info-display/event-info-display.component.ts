import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import type { ViewEventInformation } from '../../shared/server-model/view-event-information.model';

@Component({
	selector: 'event-info-display',
	styleUrls: ['./event-info-display.component.scss'],
	templateUrl: './event-info-display.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventInfoDisplayComponent {
	@Input()
	public info: ViewEventInformation;
}
