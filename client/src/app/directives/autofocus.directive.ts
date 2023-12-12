import { Directive, ElementRef, OnInit, inject } from '@angular/core';

@Directive({
  selector: '[dirAutofocus]',
  standalone: true
})
export class AutofocusDirective implements OnInit {
  private elRef = inject(ElementRef);

  ngOnInit(): void {
    setTimeout(() => this.elRef.nativeElement.focus());
  }
}
