import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NavigationExtras, Router } from '@angular/router';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const snack = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err) {
        switch (err.status) {
          case 400:
            if (err.error.errors) {
              const modelStateErrors = [];
              for (const key in err.error.errors) {
                if (err.error.errors)
                  modelStateErrors.push(err.error.errors[key]);
              }
              throw modelStateErrors;
            }
            else {
              snack.open(err.status.toString() + ': ' + err.error, 'Close', { horizontalPosition: 'end', verticalPosition: 'bottom', duration: 7000 });
            }
            break;
          case 401:
            snack.open('Unuthorized', 'Close', { horizontalPosition: 'end', verticalPosition: 'bottom', duration: 7000 });
            break;
          case 404:
            router.navigate(['/not-found']);
            break;
          case 500:
            const navigationExtras: NavigationExtras = { state: { error: err.error } };
            router.navigate(['/server-error'], navigationExtras);
            break;
          default:
            snack.open('Something unexpected went wrong.', 'Close', { horizontalPosition: 'end', verticalPosition: 'bottom', duration: 7000 });
            console.log(err);
            break;
        }
      }
      throw err;
    })
  );
};
