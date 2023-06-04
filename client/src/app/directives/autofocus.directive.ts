import { Directive, ElementRef, OnInit } from '@angular/core';

@Directive({
  selector: '[dirAutofocus]'
})
export class AutofocusDirective implements OnInit {

  constructor(private elRef: ElementRef) { }

  ngOnInit(): void {
    setTimeout(() => this.elRef.nativeElement.focus());
  }
}
