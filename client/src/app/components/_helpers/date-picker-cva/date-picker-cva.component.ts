import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-date-picker-cva',
  standalone: true,
  imports: [
    FormsModule, ReactiveFormsModule, MatInputModule,
    MatDatepickerModule, 
    MatNativeDateModule
  ],
  templateUrl: './date-picker-cva.component.html',
  styleUrls: ['./date-picker-cva.component.scss']
})
export class DatePickerCvaComponent implements ControlValueAccessor {
  @Input({ required: true }) label = '';
  @Input({ required: true }) placeHolder = '';
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
