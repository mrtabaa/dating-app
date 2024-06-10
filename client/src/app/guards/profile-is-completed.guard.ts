import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CanActivateFn, Router } from '@angular/router';
import { LoggedInUser } from '../models/logged-in-user.model';

export const profileIsCompletedGuard: CanActivateFn = () => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);
  const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

  if (loggedInUserStr) {
    const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

    if (loggedInUser.isProfileCompleted) {
      router.navigate(['/main']);

      snackBar.open("You've already completed your profile and can modify it in the 'Edit Profile' page.", 'close', { duration: 10000, horizontalPosition: 'center', verticalPosition: 'bottom' });

      return false;
    }
  }

  return true;
};
