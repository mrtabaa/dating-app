import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessagesUnreadComponent } from './messages-unread.component';

describe('MessagesUnreadComponent', () => {
  let component: MessagesUnreadComponent;
  let fixture: ComponentFixture<MessagesUnreadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MessagesUnreadComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MessagesUnreadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
