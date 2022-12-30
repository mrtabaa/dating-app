import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, ReplaySubject } from 'rxjs';
import { UserLogin } from '../_models/account/user-login.model';
import { UserRegister } from '../_models/account/user-register.model';
import { User } from '../_models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = 'https://localhost:5001/account/';

  private currentUserSource = new ReplaySubject<User | null>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  register(userInput: UserRegister): Observable<User | null> {
    return this.http.post<User>(this.baseUrl + 'register', userInput)
      .pipe(
        map(user => {
          if (user) {
            this.currentUserSource.next(user);
            localStorage.setItem('user', JSON.stringify(user));
            return user;
          }
          return null;
        })
      );
  }

  login(userInput: UserLogin): Observable<User | null> {
    return this.http.post<User>(this.baseUrl + 'login', userInput)
      .pipe(
        map(user => {
          if (user) {
            this.currentUserSource.next(user);
            localStorage.setItem('user', JSON.stringify(user));
            return user;
          }
          return null;
        })
      );
  }

  // used in app-component to set currentUserSource from the stored brwoser's localStorage key
  // now user can refresh the page or relaunch the browser without losing authentication
  setCurrentUser(user: User): void {
    this.currentUserSource.next(user);
  }

  logout(): void {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.router.navigate(['/login'])
  }
}
