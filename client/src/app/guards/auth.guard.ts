import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const snackbar = inject(MatSnackBar);

  if (localStorage.getItem('loggedInUser'))
    return true;

  localStorage.clear();

  localStorage.setItem('returnUrl', state.url);
  router.navigate([''], { queryParams: { 'returnUrl': state.url } })

  snackbar.open('Please login first.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 })

  return false;
};
