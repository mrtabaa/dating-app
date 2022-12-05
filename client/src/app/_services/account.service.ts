import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
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

  constructor(private http: HttpClient) { }

  register(userInput: UserRegister) {
    let test: Observable<UserRegister> = this.http.post<UserRegister>(this.baseUrl + 'register', userInput);

    test.subscribe(res => console.log(res));
  }

  login(userInput: UserLogin): Observable<void> {
    return this.http.post<UserProfile>(this.baseUrl + 'login', userInput).pipe(
      map((response: UserProfile) => {
        const user = response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  // used in app-component to set currentUserSource from the stored brwoser's localStorage key
  // now user can refresh the page or relaunch the browser without losing authentication
  setCurrentUser(user: UserProfile): void {
    this.currentUserSource.next(user);
  }

  logout(): void {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
