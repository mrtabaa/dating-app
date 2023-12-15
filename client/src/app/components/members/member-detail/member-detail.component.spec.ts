import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberDetailComponent } from './member-detail.component';

describe('UserDetailComponent', () => {
  let component: MemberDetailComponent;
  let fixture: ComponentFixture<MemberDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MemberDetailComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(MemberDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
