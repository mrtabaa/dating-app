import { Directive, ElementRef, Input, inject, AfterViewInit } from '@angular/core';

@Directive({
  selector: '[appAutofocusDir]',
  standalone: true
})
export class AutofocusDirective implements AfterViewInit {
  @Input('appAutofocusDir') isFocused: boolean | undefined;
  private elRef = inject(ElementRef);

  ngAfterViewInit(): void {
    if (this.isFocused) {
      this.elRef.nativeElement.focus();
    }
  }
}
