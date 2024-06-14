import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberDetailMobileComponent } from './member-detail-mobile.component';

describe('MemberDetailMobileComponent', () => {
  let component: MemberDetailMobileComponent;
  let fixture: ComponentFixture<MemberDetailMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberDetailMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MemberDetailMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
