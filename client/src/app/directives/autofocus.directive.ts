import { Directive, ElementRef, Input, OnInit, inject } from '@angular/core';

@Directive({
  selector: '[appAutofocusDir]',
  standalone: true
})
export class AutofocusDirective implements OnInit {
  @Input('appAutofocusDir') isFocused: boolean | undefined;
  private elRef = inject(ElementRef);

  ngOnInit(): void {
    if (this.isFocused)
      setTimeout(() => this.elRef.nativeElement.focus());
  }
}
