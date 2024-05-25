import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { AutofocusDirective } from '../../../directives/autofocus.directive';

@Component({
  selector: 'app-input-cva',
  standalone: true,
  imports: [
    AutofocusDirective,
    FormsModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule
  ],
  templateUrl: './input-cva.component.html',
  styleUrls: ['./input-cva.component.scss']
})
export class InputCvaComponent implements ControlValueAccessor {
  @Input({ required: true }) label = '';
  @Input() type = 'text';
  @Input({ required: true }) placeHolder = '';
  @Input() error = '';
  @Input() hint = '';
  @Input() isFocused = false;

  isPasswordHidden = true;

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

  showHidePassword(): void {
    this.isPasswordHidden = !this.isPasswordHidden;
  }
}
