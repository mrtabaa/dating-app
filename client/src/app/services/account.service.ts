import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, take } from 'rxjs';
import { UserLogin } from '../models/account/user-login.model';
import { UserRegister } from '../models/account/user-register.model';
import { environment } from '../../environments/environment';
import { LoggedInUser } from '../models/logged-in-user.model';
import { GooglePlacesService } from './google-places.service';
import { ResponsiveService } from './responsive.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private googlePlacesService = inject(GooglePlacesService);
  private responsiveService = inject(ResponsiveService);

  private baseUrl = environment.apiUrl + "account/";

  loggedInUserSig = signal<LoggedInUser | undefined>(undefined);

  constructor(private http: HttpClient, private router: Router) { }

  register(userInput: UserRegister): Observable<LoggedInUser | null> {
    return this.http.post<LoggedInUser>(this.baseUrl + 'register', userInput)
      .pipe(
        map((user: LoggedInUser) => {
          if (user) {
            this.setCurrentUser(user);
            return user;
          }
          return null;
        })
      );
  }

  login(userInput: UserLogin): Observable<LoggedInUser | null> {
    return this.http.post<LoggedInUser>(this.baseUrl + 'login', userInput)
      .pipe(
        map((user: LoggedInUser) => {
          if (user) {
            this.setCurrentUser(user);

            this.setGetReturnUrl(); // Never put it setCurrentUser() or all pages refreshes land on members only. 

            return user;
          }
          return null;
        })
      );
  }

  registerDemo(userInput: UserRegister): Observable<LoggedInUser | null> {
    return this.http.post<LoggedInUser>(this.baseUrl + 'register', userInput)
      .pipe(
        map((user: LoggedInUser) => { return user ? user : null })
      );
  }

  /**
   * Check if user's token is still valid or log them out. 
   * Called in app.component.ts
   * @returns Observable<LoggedInUser | null>
   */
  reloadLoggedInUser(): void {
    if (localStorage.getItem("loggedInUser"))
      this.http.get<LoggedInUser>(this.baseUrl)
        .pipe(take(1)).subscribe({
          next: (loggedInUser: LoggedInUser) => this.setCurrentUser(loggedInUser), // set loggedInUser
          error: () => this.logout()
        });
  }

  logout(): void {
    localStorage.clear();
    this.loggedInUserSig.set(undefined);
    this.router.navigate(['account/login'])
    this.googlePlacesService.resetCountry();
  }

  /**
   * Used in app-component to set currentUserSig
   * Now user can refresh the page or relaunch the browser without losing authentication or returnUrl
   * @param loggedInUser 
   */
  setCurrentUser(loggedInUser: LoggedInUser): void {
    localStorage.setItem('loggedInUser', JSON.stringify(loggedInUser));

    this.setLoggedInUserRoles(loggedInUser);

    this.loggedInUserSig.set(loggedInUser);

    // Set it to false to show Hallboard in color to the loggedInUser. Default was true
    this.responsiveService.isWelcomeCompSig.set(false);
  }

  setGetReturnUrl(): void {
    const returnUrl: string | null = localStorage.getItem('returnUrl');

    this.loggedInUserSig()?.roles.includes('admin')
      ? this.router.navigate(['/admin'])
      : returnUrl
        ? this.router.navigate([returnUrl])
        : this.router.navigate(['/main']);

    localStorage.removeItem('returnUrl');
  }

  setLoggedInUserRoles(loggedInUser: LoggedInUser): void {
    loggedInUser.roles = [];

    const roles = JSON.parse(atob(loggedInUser.token.split('.')[1])).role; // get the token's 2nd part then select role

    Array.isArray(roles) ? loggedInUser.roles = roles : loggedInUser.roles.push(roles);
  }
}
