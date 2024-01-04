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

  currentUserSig = signal<LoggedInUser | null>(null);

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

   // get logged-in user when browser is refreshed
   getLoggedInUser(): Observable<LoggedInUser | null> {
    return this.http.get<LoggedInUser>(this.baseUrl).pipe(
      map((loggedInUserResponse: LoggedInUser | null) => {
        if (loggedInUserResponse)
          return loggedInUserResponse

        return null;
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSig.set(null);
    this.router.navigate(['account/login'])
  }

  // used in app-component to set currentUserSource from the stored brwoser's localStorage key
  // now user can refresh the page or relaunch the browser without losing authentication or returnUrl
  setCurrentUser(loggedInUser: LoggedInUser): void {
    localStorage.setItem('token', loggedInUser.token);

    this.currentUserSig.set(loggedInUser);
  }

  setGetReturnUrl(): void {
    const returnUrl: string | null = localStorage.getItem('returnUrl');

    returnUrl
      ? this.router.navigate([returnUrl])
      : this.router.navigate(['members']);

    localStorage.removeItem('returnUrl');
  }
}
