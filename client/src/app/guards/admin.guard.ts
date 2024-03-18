import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoggedInUser } from '../models/logged-in-user.model';

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  const loggedInUserStr = localStorage.getItem('loggedInUser');
  if (!loggedInUserStr) return false;

  const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

  const roles = JSON.parse(atob(loggedInUser.token.split('.')[1])).role;

  if (roles?.some((role: string) => role === "admin" || role === 'moderator'))
    return true;

  snackBar.open("You're not allowed here. Only admins.", "Close", { verticalPosition: "top", horizontalPosition: "center", duration: 7000 });

  router.navigate(['no-access']);

  return false;
};
