import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CanActivateFn, Router } from '@angular/router';
import { LoggedInUser } from '../models/logged-in-user.model';

export const profileCompleteGuard: CanActivateFn = () => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);
  const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

  if (loggedInUserStr) {
    const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

    if (loggedInUser.isProfileCompleted)
      return true;
    
    router.navigate(['account/complete-profile']);
    
    snackBar.open("Please complete your profile first.", 'close', { duration: 7000, horizontalPosition: 'center', verticalPosition: 'top' });
  }

  return false;
};
