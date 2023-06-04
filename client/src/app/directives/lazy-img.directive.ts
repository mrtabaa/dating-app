import { Directive, ElementRef } from '@angular/core';

@Directive({ selector: 'img' })
export class LazyImgDirective {
  constructor({ nativeElement }: ElementRef<HTMLImageElement>) {
    const supports = 'loading' in HTMLImageElement.prototype;

    if (supports) {
      // supports mordern browsers only
      nativeElement.setAttribute('loading', 'lazy');
    } else {
      // fallback to IntersectionObserver from this link to support all browsers
      // https://developer.mozilla.org/en-US/docs/Web/API/Intersection_Observer_API
    }
  }
}