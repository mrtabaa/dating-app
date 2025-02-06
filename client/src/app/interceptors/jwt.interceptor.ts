import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';
import {AccountService} from "../services/account.service";
import {inject} from "@angular/core";
import {catchError, switchMap, throwError} from 'rxjs';
import {MatSnackBar} from "@angular/material/snack-bar";

let refreshTokenInProgress = false;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);
  const snack = inject(MatSnackBar);

  const loggedInUserStr = localStorage.getItem('loggedInUser');
  if (loggedInUserStr) {
    req = req.clone({withCredentials: true});
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
            return next(req.clone({withCredentials: true}));
          }),
          catchError((error) => {
            console.error(error);
            refreshTokenInProgress = false; // Reset flag if refresh fails
            return throwError(() => error);
          })
        );
      }

      snack.open('Invalid credentials. Please login again.', 'Close', {horizontalPosition: 'center', verticalPosition: 'top', duration: 7000});
      // accountService.logout();
      return throwError(() => err);
    })
  );
};

