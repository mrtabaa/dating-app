import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoggedInUser } from '../models/logged-in-user.model';

export const authLoggedInGuard: CanActivateFn = () => {
  const snackbar = inject(MatSnackBar);
  const router = inject(Router);

  const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

  if (loggedInUserStr) {
    snackbar.open('You are already logged in.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });

    const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

    if (!loggedInUser.isProfileCompleted)
      router.navigate(['account/complete-profile']);
    else
      router.navigate(['/main']);

    return false;
  }

  return true;
};
