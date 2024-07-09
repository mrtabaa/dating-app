import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessagesInboxComponent } from './messages-inbox.component';

describe('MessagesInboxComponent', () => {
  let component: MessagesInboxComponent;
  let fixture: ComponentFixture<MessagesInboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MessagesInboxComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MessagesInboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
