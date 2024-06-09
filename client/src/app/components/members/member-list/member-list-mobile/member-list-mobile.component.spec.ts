import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberListMobileComponent } from './member-list-mobile.component';

describe('MemberListMobileComponent', () => {
  let component: MemberListMobileComponent;
  let fixture: ComponentFixture<MemberListMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberListMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MemberListMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
