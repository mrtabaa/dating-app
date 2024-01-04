import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable } from 'rxjs';
import { UserLogin } from '../models/account/user-login.model';
import { UserRegister } from '../models/account/user-register.model';
import { environment } from '../../environments/environment';
import { LoggedInUser } from '../models/logged-in-user.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = environment.apiUrl + "account/";

  loggedInUserSig = signal<LoggedInUser | null>(null);

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

            this.setGetReturnUrl();

            return user;
          }
          return null;
        })
      );
  }

  /**
   * Get logged-in user when browser is refreshed
   * Called in app.component.ts
   * @returns Observable<LoggedInUser | null>
   */
  getLoggedInUser(): Observable<LoggedInUser | null> {
    return this.http.get<LoggedInUser>(this.baseUrl);
  }

  logout(): void {
    localStorage.clear();
    this.loggedInUserSig.set(null);
    this.router.navigate(['account/login'])
  }

  /**
   * Used in app-component to set currentUserSig
   * Now user can refresh the page or relaunch the browser without losing authentication or returnUrl
   * @param loggedInUser 
   */
  setCurrentUser(loggedInUser: LoggedInUser): void {
    localStorage.setItem('token', loggedInUser.token);

    this.loggedInUserSig.set(loggedInUser);
  }

  setGetReturnUrl(): void {
    const returnUrl: string | null = localStorage.getItem('returnUrl');

    returnUrl
      ? this.router.navigate([returnUrl])
      : this.router.navigate(['members']);

    localStorage.removeItem('returnUrl');
  }
}
