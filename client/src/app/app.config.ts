import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './interceptors/error.interceptor';
import { jwtInterceptor } from './interceptors/jwt.interceptor';
import { loadingInterceptor } from './interceptors/loading.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { RECAPTCHA_V3_SITE_KEY } from "ng-recaptcha";

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes), provideAnimations(),
    provideHttpClient(withInterceptors([errorInterceptor, jwtInterceptor, loadingInterceptor])), provideAnimationsAsync(), provideAnimationsAsync(), provideAnimationsAsync(), provideAnimationsAsync(),
    { provide: RECAPTCHA_V3_SITE_KEY, useValue: "6LfuEvopAAAAAGYnpSebn_EDgAtNKjcx1YSOPTyv" }
  ]
};
