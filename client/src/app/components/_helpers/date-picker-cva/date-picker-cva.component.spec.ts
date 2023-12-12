import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DatePickerCvaComponent } from './date-picker-cva.component';

describe('DatePickerCvaComponent', () => {
  let component: DatePickerCvaComponent;
  let fixture: ComponentFixture<DatePickerCvaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [DatePickerCvaComponent]
    });
    fixture = TestBed.createComponent(DatePickerCvaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
