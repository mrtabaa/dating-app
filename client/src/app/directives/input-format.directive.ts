import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[dirInputFormat]'
})
export class InputFormatDirective {

  constructor(private el: ElementRef) { }

  @Input('dirInputFormat') format!: string;

  @HostListener('blur')
  onBlur() {
    let value: string = this.el.nativeElement.value;

    if (this.format == 'uppercase')
      this.el.nativeElement.value = value.toUpperCase();
    else
      this.el.nativeElement.value = value.toLowerCase();
  }
}
