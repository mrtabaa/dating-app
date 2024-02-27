import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

export const adminGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  const token = localStorage.getItem('token');
  if (!token) return false;

  const roles = JSON.parse(atob(token.split('.')[1])).role;

  if (roles?.some((role: string) => role === "admin" || role === 'moderator'))
    return true;

  snackBar.open("You're not allowed here. Only admins.", "Close", { verticalPosition: "top", horizontalPosition: "center", duration: 7000 });

  router.navigate(['no-access']);

  return false;
};
