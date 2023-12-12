import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';

@Directive({
  selector: '[dirInputFormat]',
  standalone: true
})
export class InputFormatDirective {
  private elRef = inject(ElementRef);

  @Input('dirInputFormat') format!: string;

  @HostListener('blur')
  onBlur() {
    let value: string = this.elRef.nativeElement.value;

    if (this.format == 'uppercase')
      this.elRef.nativeElement.value = value.toUpperCase();
    else
      this.elRef.nativeElement.value = value.toLowerCase();
  }
}
