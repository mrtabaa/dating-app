import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';
import {LoggedInUser} from '../models/logged-in-user.model';
import {AccountService} from "../services/account.service";
import {inject} from "@angular/core";
import {catchError, switchMap, throwError} from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);

  const loggedInUserStr = localStorage.getItem('loggedInUser');
  if (loggedInUserStr) {
    const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

    req = req.clone({withCredentials: true});
  }

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) { // Unauthorized
        // Get new tokens using refresh-token. Refresh token is only sent when this path is requested.
        // Path = "/api/account/refresh-tokens"
        return accountService.refreshTokens()
          .pipe(
            switchMap(() => next(req.clone({withCredentials: true})))
          );
      }

      return throwError(() => err);
    })
  );
};
