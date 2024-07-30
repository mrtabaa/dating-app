import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessagesMobileComponent } from './messages-mobile.component';

describe('MessagesMobileComponent', () => {
  let component: MessagesMobileComponent;
  let fixture: ComponentFixture<MessagesMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MessagesMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MessagesMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
