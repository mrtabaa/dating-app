import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberCardMobileComponent } from './member-card-mobile.component';

describe('MemberCardMobileComponent', () => {
  let component: MemberCardMobileComponent;
  let fixture: ComponentFixture<MemberCardMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberCardMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MemberCardMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
