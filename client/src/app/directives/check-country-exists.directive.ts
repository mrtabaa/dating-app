import { Directive, HostListener, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Directive({
  selector: '[appCheckCountryExistsDir]',
  standalone: true
})
// check if the user input country exists
export class CheckCountryExistsDirective {
  // Pass in multiple inputs: two controllers
  @Input() selectedCountryCtrlIn: AbstractControl | undefined;
  @Input() countryCtrlIn: AbstractControl | undefined;

  @HostListener('focusout')
  onBlur() {
    if (this.selectedCountryCtrlIn && this.countryCtrlIn)
      if (this.selectedCountryCtrlIn.invalid)
        this.countryCtrlIn.setValue("");
  }
}
