import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';

@Directive({
  selector: '[appInputFormatDir]',
  standalone: true
})
export class InputFormatDirective {
  private elRef = inject(ElementRef);

  @Input('appInputFormatDir') format!: string;

  @HostListener('blur')
  onBlur() {
    const value: string = this.elRef.nativeElement.value;

    if (this.format == 'uppercase')
      this.elRef.nativeElement.value = value.toUpperCase();
    else
      this.elRef.nativeElement.value = value.toLowerCase();
  }
}
