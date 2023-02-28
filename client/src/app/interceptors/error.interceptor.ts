import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { catchError, Observable } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private snack: MatSnackBar) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
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
                this.snack.open(err.status.toString() + ': ' + err.error , 'Close', {duration: 7000});
              }
              break;
            case 401:
              this.snack.open('Unuthorized', 'Close', { duration: 7000 });
              break;
            case 404:
              this.router.navigate(['/not-found']);
              break;
            case 500:
              const navigationExtras: NavigationExtras = { state: { error: err.error } };
              this.router.navigate(['/server-error'], navigationExtras);
              break;
            default:
              this.snack.open('Something unexpected went wrong.', 'Close' , { duration: 7000 });
              console.log(err);
              break;
          }
        }
        throw err;
      })
    );
  }
}
