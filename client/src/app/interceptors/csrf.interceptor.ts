import {HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest} from '@angular/common/http';
import {inject} from '@angular/core';
import {AccountService} from '../services/account.service';
import {Observable, switchMap, take} from 'rxjs';

export const csrfInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const accountService = inject(AccountService);
  const csrfToken = sessionStorage.getItem('csrfToken');

  // Only intercept unsafe methods
  if (!['POST', 'PUT', 'DELETE'].includes(req.method)) {
    return next(req);
  }

  // If a token already exists, add it and proceed
  if (csrfToken) {
    const modifiedReq = req.clone({
      headers: req.headers.set('X-XSRF-TOKEN', csrfToken),
    });
    return next(modifiedReq);
  }

  // If no token, fetch it, add to session and request, then proceed
  return accountService.getCsrfToken$().pipe(
    take(1),
    switchMap(response => {
      sessionStorage.setItem('csrfToken', response.requestToken);
      const modifiedReq = req.clone({
        headers: req.headers.set('X-XSRF-TOKEN', response.requestToken),
      });
      return next(modifiedReq);
    })
  );
};
