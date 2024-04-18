import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

export const authLoggedInGuard: CanActivateFn = () => {
  const router = inject(Router);
  const snackbar = inject(MatSnackBar);

  if (localStorage.getItem('loggedInUser')) {
    snackbar.open('You are already logged in.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
    router.navigate(['members']);

    return false;
  }

  return true;
};
