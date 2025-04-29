import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { AccountService } from "../services/account.service";
import { inject } from "@angular/core";
import { catchError, switchMap, throwError } from 'rxjs';
import { MatSnackBar } from "@angular/material/snack-bar";

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
      if (err.status === 401 && !refreshTokenInProgress) { // Unauthorized
        // Prevent multiple refresh token requests by setting the flag
        refreshTokenInProgress = true;

        // Get new tokens using refresh-token. Refresh token is only sent when this path is requested.
        return accountService.refreshTokens().pipe(
          switchMap(() => {
            refreshTokenInProgress = false;
            return next(req);
          }),
          catchError((error) => {
            refreshTokenInProgress = false; // Reset flag if refresh fails
            accountService.logout();

            snack.open('Your session has expired. Please login again.', 'Close', {
              horizontalPosition: 'center',
              verticalPosition: 'top',
              duration: 7000
            });

            return throwError(() => error);
          })
        );
      }

      return throwError(() => err);
    })
  );
};

