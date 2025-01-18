import {HttpClient} from '@angular/common/http';
import {inject, Injectable, signal} from '@angular/core';
import {Router} from '@angular/router';
import {map, Observable, take} from 'rxjs';
import {UserLogin} from '../models/account/user-login.model';
import {UserRegister} from '../models/account/user-register.model';
import {environment} from '../../environments/environment';
import {LoggedInUser} from '../models/logged-in-user.model';
import {GooglePlacesService} from './google-places.service';
import {ResponsiveService} from './responsive.service';
import {PresenceService} from './hubs/presence.service';
import {Verify} from "../models/account/verify.model";
import {MatSnackBar} from "@angular/material/snack-bar";
import {CommonService} from "./common.service";
import {ApiResponseMessage} from "../models/helpers/api-response-message";
import {RecoveryValidationRequest} from "../models/account/recovery-validation-request.model";
import {ResetPassword} from "../models/account/reset-password.model";

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  loggedInUserSig = signal<LoggedInUser | undefined>(undefined);
  private _googlePlacesService = inject(GooglePlacesService);
  private _responsiveService = inject(ResponsiveService);
  private _http = inject(HttpClient);
  private _router = inject(Router);
  private _presenceService = inject(PresenceService);
  private baseUrl = environment.apiUrl + "account/";
  private _snackBar = inject(MatSnackBar);
  private _isVerifyingAccount = inject(CommonService).isVerifyingAccountSig;


  register(userInput: UserRegister): Observable<void> {
    return this._http.post<boolean>(this.baseUrl + 'register', userInput)
      .pipe(
        map((isRegistered: boolean) => {
          if (isRegistered) {
            localStorage.setItem('email', userInput.email);
            this._isVerifyingAccount.set(true);
          }
        })
      );
  }

  verify(verify: Verify): Observable<void> {
    return this._http.post<LoggedInUser>(this.baseUrl + 'verify', verify)
      .pipe(
        map((user: LoggedInUser) => {
          if (user) {
            this.setCurrentUser(user);
            localStorage.removeItem('email');
            this._router.navigate(['/main']);
            this._isVerifyingAccount.set(false);
            this._snackBar.open("You are logged in as: " + user?.userName, "Close", {
              verticalPosition: 'bottom',
              horizontalPosition: 'center',
              duration: 7000
            })
          }
        })
      );
  }

  resendVerifyCode(resendRequest: RecoveryValidationRequest): Observable<void> {
    return this._http.post<boolean>(this.baseUrl + 'resend-verify-code', resendRequest)
      .pipe(
        map((res: boolean) => {
          if (res) {
            console.log(res);
          }
        })
      )
  }

  login(userInput: UserLogin): Observable<LoggedInUser | null> {
    return this._http.post<LoggedInUser>(this.baseUrl + 'login', userInput)
      .pipe(
        map((user: LoggedInUser) => {
          if (user) {
            if (user.isEmailNotConfirmed) {
              localStorage.setItem('email', user.email);
              this._isVerifyingAccount.set(true);
            } else {
              this._isVerifyingAccount.set(false);
              this._snackBar.open('You logged in as: ' + user?.userName, 'Close', {
                verticalPosition: 'bottom',
                horizontalPosition: 'center',
                duration: 7000
              })
              this.setCurrentUser(user);
              this.setGetReturnUrl(); // Never put it setCurrentUser() or all pages refreshes land on members only.
              return user;
            }
          }
          return null;
        })
      );
  }

  registerDemo(userInput: UserRegister): Observable<LoggedInUser | null> {
    return this._http.post<LoggedInUser>(this.baseUrl + 'register', userInput)
      .pipe(
        map((user: LoggedInUser) => {
          return user ? user : null
        })
      );
  }

  /**
   * Check if user's token is still valid or log them out.
   * Called in app.component.ts
   * @returns Observable<LoggedInUser | null>
   */
  reloadLoggedInUser(): void {
    if (localStorage.getItem("loggedInUser"))
      this._http.get<LoggedInUser>(this.baseUrl)
        .pipe(take(1)).subscribe({
        next: (loggedInUser: LoggedInUser) => this.setCurrentUser(loggedInUser), // set loggedInUser
        error: () => this.logout()
      });
  }

  requestResetPassword(request: RecoveryValidationRequest): Observable<ApiResponseMessage> {
    return this._http.post<ApiResponseMessage>(this.baseUrl + 'request-reset-password', request)
      .pipe(
        map(res => {
          this._snackBar.open(res.message, 'Close', {
            verticalPosition: 'top',
            horizontalPosition: 'center',
            duration: 10000
          });

          this._router.navigate(['/']);

          return res;
        })
      );
  }

  resetPassword(resetPassword: ResetPassword): Observable<ApiResponseMessage> {
    return this._http.post<ApiResponseMessage>(this.baseUrl + 'reset-password', resetPassword)
      .pipe(
        map(res => {
          if (res)
            this._snackBar.open(res.message, 'Close', {
              verticalPosition: 'bottom',
              horizontalPosition: 'center',
              duration: 7000
            });

          return res;
        })
      );
  }

  logout(): void {
    localStorage.clear();
    this.loggedInUserSig.set(undefined);
    this._router.navigate(['account/login']);
    this._googlePlacesService.resetCountry();
    this._presenceService.stopHubConnection();
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

    this._presenceService.createHubConnection(loggedInUser);

    // Set it to false to show Hallboard in color to the loggedInUser. Default was true
    this._responsiveService.isWelcomeCompSig.set(false);
  }

  setGetReturnUrl(): void {
    const returnUrl: string | null = localStorage.getItem('returnUrl');

    this.loggedInUserSig()?.roles.includes('admin')
      ? this._router.navigate(['/admin'])
      : returnUrl
        ? this._router.navigate([returnUrl])
        : this._router.navigate(['/main']);

    localStorage.removeItem('returnUrl');
  }

  setLoggedInUserRoles(loggedInUser: LoggedInUser): void {
    loggedInUser.roles = [];

    const roles = JSON.parse(atob(loggedInUser.token.split('.')[1])).role; // get the token's 2nd part then select role

    Array.isArray(roles) ? loggedInUser.roles = roles : loggedInUser.roles.push(roles);
  }
}
