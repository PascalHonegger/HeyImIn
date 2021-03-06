import { TestBed, async } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { RouterTestingModule } from '@angular/router/testing';

describe('AppComponent', () => {
	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [RouterTestingModule.withRoutes([])],
			declarations: [ AppComponent ]
		}).compileComponents();
	}));

	it('should create the app', () => {
		const fixture = TestBed.createComponent(AppComponent);
		const app = fixture.componentInstance;
		expect(app).toBeTruthy();
	});

	it('should render router outlet', () => {
		const fixture = TestBed.createComponent(AppComponent);
		const compiled = fixture.nativeElement;
		expect(compiled.querySelector('router-outlet')).toBeTruthy();
	});
});
