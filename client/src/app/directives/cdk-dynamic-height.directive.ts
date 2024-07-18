import { AfterViewInit, Directive, ElementRef, inject, Input, OnDestroy } from '@angular/core';

@Directive({
  selector: '[appCdkDynamicHeightDir]',
  standalone: true
})
export class CdkDynamicHeightDirective implements AfterViewInit, OnDestroy {
  @Input() appCdkDynamicHeightDir: { height: number } | undefined;
  private _elementRef = inject(ElementRef);
  private _resizeObserver: ResizeObserver | undefined;

  ngAfterViewInit() {
    this._resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        if (this.appCdkDynamicHeightDir && entry.contentRect.height !== this.appCdkDynamicHeightDir.height) {
          this.appCdkDynamicHeightDir.height = entry.contentRect.height;
        }
      }
    });
    this._resizeObserver.observe(this._elementRef.nativeElement);
  }

  ngOnDestroy() {
    if (this._resizeObserver) {
      this._resizeObserver.disconnect();
    }
  }
}
