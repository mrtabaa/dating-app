import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { AccountService } from '../services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private accountService: AccountService, private router: Router, private snackbar: MatSnackBar) { }
  
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    
    return this.accountService.currentUser$.pipe(
      map(user => {
        if (user) return true;

        localStorage.setItem('returnUrl', state.url);
        this.router.navigate(['/login'], { queryParams: { 'returnUrl': state.url } })
        
        this.snackbar.open('Please login first.', 'Close', {horizontalPosition: 'end', duration: 7000})
        return false;
      })
    );
  }
}
