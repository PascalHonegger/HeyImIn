import { isDevMode } from '@angular/core';

const baseUrlDebug: string = 'http://localhost:51203/api/';

// Used for testing
// const baseUrlProd: string = 'http://hey-im-in.azurewebsites.net/api/';
const baseUrlProd: string = '../api/';

export abstract class ServerClientBase {
	protected readonly baseUrl: string;

	constructor(controllerPrefix: string) {
		this.baseUrl = (isDevMode() ? baseUrlDebug : baseUrlProd) + controllerPrefix;
	}
}
