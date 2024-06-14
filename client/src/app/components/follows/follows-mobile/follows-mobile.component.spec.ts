import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FollowsMobileComponent } from './follows-mobile.component';

describe('FollowsMobileComponent', () => {
  let component: FollowsMobileComponent;
  let fixture: ComponentFixture<FollowsMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FollowsMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(FollowsMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
