import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoadingService } from '../services/loading.service';
import { delay, finalize } from 'rxjs';
import { environment } from '../../environments/environment';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  loadingService.loading(); // loading starts

  return next(req).pipe(
    environment.production ? delay(0) : delay(500), // delay on dev only
    finalize(() => {
      loadingService.idle(); // loading ends
    })
  );
};
