import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-date-picker-cva',
  templateUrl: './date-picker-cva.component.html',
  styleUrls: ['./date-picker-cva.component.scss']
})
export class DatePickerCvaComponent implements ControlValueAccessor {
  @Input('label') label = '';
  @Input('type') type = 'text';
  @Input('placeHolder') placeHolder = '';
  @Input('hint') hint = '';
  @Input('minDate') minDate: Date | undefined;
  @Input('maxDate') maxDate: Date | undefined;

  constructor(@Self() public ngControl: NgControl) {
    ngControl.valueAccessor = this;
  }

  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }

  get Control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
