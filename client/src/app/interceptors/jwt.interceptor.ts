import {HttpErrorResponse, HttpInterceptorFn, HttpRequest} from '@angular/common/http';
import {AccountService} from "../services/account.service";
import {inject} from "@angular/core";
import {catchError, switchMap, throwError} from 'rxjs';
import {MatSnackBar} from "@angular/material/snack-bar";

let refreshTokenInProgress = false;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);
  const snack = inject(MatSnackBar);

  const loggedInUserStr = localStorage.getItem('loggedInUser');
  if (!loggedInUserStr) {
    accountService.logout();
    return next(req); // {withCredentials: true} applied in credentialsInterceptor
  }

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !refreshTokenInProgress) {
        refreshTokenInProgress = true;

        return accountService.refreshTokens().pipe(
          switchMap(() => {
            refreshTokenInProgress = false;

            // Delete old cookie (so Angular doesn't send a mismatched one)
            document.cookie = 'XSRF-TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';

            // Trigger backend to set the new CSRF cookie
            return accountService.getCsrfToken$().pipe(
              switchMap((newTokenResponse) => {
                // Store latest CSRF request token
                sessionStorage.setItem('csrfToken', newTokenResponse.requestToken);

                // Clone the original request with the **updated X-XSRF-TOKEN**
                const updatedReq: HttpRequest<any> = req.clone({
                  headers: req.headers.set('X-XSRF-TOKEN', newTokenResponse.requestToken),
                });

                // Retry the original request with fresh CSRF token
                return next(updatedReq);
              })
            );
          }),
          catchError((error) => {
            refreshTokenInProgress = false;
            accountService.logout();

            snack.open('Your session has expired. Please login again.', 'Close', {
              horizontalPosition: 'center',
              verticalPosition: 'top',
              duration: 7000,
            });

            return throwError(() => error);
          })
        );
      }

      return throwError(() => err);
    })
  );
};

