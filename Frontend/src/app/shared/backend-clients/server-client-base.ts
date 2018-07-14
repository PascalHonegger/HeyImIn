import { isDevMode } from '@angular/core';

const baseUrlDebug = 'http://localhost:51203/api/';

// Used for testing
// const baseUrlProd: string = 'http://hey-im-in.azurewebsites.net/api/';
const baseUrlProd = '/api/';

export abstract class ServerClientBase {
	protected readonly baseUrl: string;

	constructor(controllerPrefix: string) {
		this.baseUrl = (isDevMode() ? baseUrlDebug : baseUrlProd) + controllerPrefix;
	}
}
