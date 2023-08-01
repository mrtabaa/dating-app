import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, FormControl } from '@angular/forms';

@Component({
  selector: 'app-input-cva',
  templateUrl: './input-cva.component.html',
  styleUrls: ['./input-cva.component.scss']
})
export class InputCvaComponent implements ControlValueAccessor {
  @Input('label') label = '';
  @Input('type') type = 'text';
  @Input('placeHolder') placeHolder = '';
  @Input('hint') hint = '';
  @Input('isFocused') isFocused = false;

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
