import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
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
              snack.open(err.error, 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
            }
            break;
          case 401:
            if (localStorage.getItem("loggedInUser")) {
              snack.open(err.error, 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
              router.navigate(['account/login'])
            }
            break;
          case 403:
            router.navigate(['/no-access']);
            break;
          case 404:
            router.navigate(['/not-found']);
            break;
          case 500:
            router.navigate(['/server-error'], { state: { error: err.error } });
            break;
          default:
            snack.open('Something unexpected went wrong.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
            console.log(err);
            break;
        }
      }
      throw err;
    })
  );
};
