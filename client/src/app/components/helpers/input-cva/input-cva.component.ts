import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, FormControl } from '@angular/forms';

@Component({
  selector: 'app-input-cva',
  templateUrl: './input-cva.component.html',
  styleUrls: ['./input-cva.component.scss']
})
export class InputCvaComponent implements ControlValueAccessor {
  @Input({ required: true }) label = '';
  @Input() type = 'text';
  @Input({ required: true }) placeHolder = '';
  @Input() hint = '';
  @Input() isFocused = false;

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
