import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, ReplaySubject } from 'rxjs';
import { UserLogin } from '../_models/account/user-login.model';
import { UserRegister } from '../_models/account/user-register.model';
import { UserProfile } from '../_models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = 'https://localhost:5001/account/';

  private currentUserSource = new ReplaySubject<UserProfile | null>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  register(userInput: UserRegister): Observable<UserRegister> {
    return this.http.post<UserRegister>(this.baseUrl + 'register', userInput);
  }

  login(userInput: UserLogin): Observable<UserProfile | null> {
    return this.http.post<UserProfile>(this.baseUrl + 'login', userInput).pipe(
      map((user: UserProfile) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
          return user;
        }
        
        return null;
      }));
  }

  // used in app-component to set currentUserSource from the stored brwoser's localStorage key
  // now user can refresh the page or relaunch the browser without losing authentication
  setCurrentUser(user: UserProfile): void {
    this.currentUserSource.next(user);
  }

  logout(): void {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.router.navigate(['/login'])
  }
}
