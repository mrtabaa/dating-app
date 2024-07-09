import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessagesSentComponent } from './messages-sent.component';

describe('MessagesSentComponent', () => {
  let component: MessagesSentComponent;
  let fixture: ComponentFixture<MessagesSentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MessagesSentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MessagesSentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
