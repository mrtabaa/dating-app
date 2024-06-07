import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavMobileComponent } from './nav-mobile.component';

describe('NavMobileComponent', () => {
  let component: NavMobileComponent;
  let fixture: ComponentFixture<NavMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(NavMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
