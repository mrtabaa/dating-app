import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthLoggedInGuard implements CanActivate {
  constructor(private accountService: AccountService, private router: Router, private matSnack: MatSnackBar) { }
  
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.accountService.currentUser$.pipe(
      map(user => {
        if (user) {
          this.router.navigate(['/members']);
          this.matSnack.open('You are already logged in', 'Close', { horizontalPosition: 'center', duration: 7000 })
          return false;
        }
        return true;
      })
    );
  }
}
