import { Directive, HostListener, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Directive({
  selector: '[appCheckCountryExistsDir]',
  standalone: true
})
// check if the user input country exists
export class CheckCountryExistsDirective {
  // Pass in multiple inputs: two controllers
  @Input('checkCountryExistsIn') selectedCountryCtrl!: AbstractControl;
  @Input('secondInputIn') countryFilterCtrl!: AbstractControl;

  @HostListener('focusout')
  onBlur() {
    if (this.selectedCountryCtrl.invalid)
      this.countryFilterCtrl.setValue("");
  }

}
