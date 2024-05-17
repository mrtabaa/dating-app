import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

export const authLoggedInGuard: CanActivateFn = () => {
  const snackbar = inject(MatSnackBar);

  if (localStorage.getItem('loggedInUser')) {
    snackbar.open('You are already logged in.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });

    return false;
  }

  return true;
};
