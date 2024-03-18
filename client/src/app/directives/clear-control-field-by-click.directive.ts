import { Directive, HostListener, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Directive({
  selector: '[appClearControlFieldByClickDir]',
  standalone: true
})
export class ClearControlFieldByClickDirective {

  // send control from DOM through this line, e.g, [dirClearControlFieldByClick]="CountryFilterCtrl"
  @Input('appClearControlFieldByClickDir') control!: AbstractControl;

  @HostListener('click', ['$event.target'])
  public onClickOrEnter(): void { 
    if (this.control.value) {
      this.control.setValue("");
    }
  }
}
