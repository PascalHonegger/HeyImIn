import { AppPage } from './app.po';

describe('Start page', () => {
	let page: AppPage;

	beforeEach(() => {
		page = new AppPage();
	});

	it('should display page title', () => {
		page.navigateTo();
		expect(page.getTitleText()).toEqual("HEY, I'M IN");
	});
});
