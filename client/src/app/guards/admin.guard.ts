import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AccountService } from '../services/account.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  if (accountService.loggedInUserSig()?.roles.some(role => role === "admin" || role === 'moderator'))
    return true;

  snackBar.open("You're not allowed here. Only admins.", "Close", { verticalPosition: "top", horizontalPosition: "center", duration: 7000 });

  router.navigate(['no-access']);

  return false;
};
