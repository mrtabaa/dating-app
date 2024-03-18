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
  @Input() hint = '';
  @Input() minDate: Date | undefined;
  @Input() maxDate: Date | undefined;

  constructor(@Self() public ngControl: NgControl) {
    ngControl.valueAccessor = this;
  }

  writeValue(obj: unknown): void {
    obj // avoiding lint error
  }
  registerOnChange(fn: unknown): void {
    fn // avoiding lint error
  }
  registerOnTouched(fn: unknown): void {
    fn // avoiding lint error
  }

  get Control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
