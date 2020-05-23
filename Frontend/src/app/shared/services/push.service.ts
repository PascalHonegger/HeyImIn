import { Injectable } from '@angular/core';
import { SwPush } from '@angular/service-worker';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
	providedIn: 'root'
})
export class PushService {
	public pushEnabledInThisBrowser: boolean;

	private readonly VAPID_PUBLIC_KEY = 'BL4YMDoruCzMfZpjXYQ2FpDNoeIoadnbtC4uhqAblCH1NLAvxRXmDY9XT9QwtU8yuqAq87-DwA7goHpS6pcc70g';

	constructor(private push: SwPush, private snackBar: MatSnackBar) {
		this.pushEnabledInThisBrowser = push.isEnabled;
		if (!this.pushEnabledInThisBrowser) {
			console.warn('Push notifications not enabled / supported');
			return;
		}

		// TODO this.push.subscription.

		this.push.messages.subscribe((v) => {
			console.warn(v);
			this.snackBar.open('Meeesaaage=' + v);
		});
	}

	public addDevice(): void {
		this.push.requestSubscription({ serverPublicKey: this.VAPID_PUBLIC_KEY })
			.then(sub => console.info(sub))
			.catch(err => console.error(err));
	}
}
