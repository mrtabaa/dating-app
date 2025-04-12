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
import {Roles} from "../enums/Roles.enum";

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
  private _isVerifyingAccountSig = inject(CommonService).isVerifyingAccountSig;

  getCsrfToken$(): Observable<{ requestToken: string }> {
    return this._http.get<{ requestToken: string }>(this.baseUrl + 'get-csrf-token');
  }

  register(userInput: UserRegister): Observable<void> {
    return this._http.post<boolean>(this.baseUrl + 'register', userInput)
      .pipe(
        map((isRegisterSuccess: boolean) => {
          if (isRegisterSuccess) {
            sessionStorage.setItem('email', userInput.email);
            this._isVerifyingAccountSig.set(true);
          }
        })
      );
  }

  verify(verify: Verify): Observable<void> {
    return this._http.post<LoggedInUser>(this.baseUrl + 'verify', verify)
      .pipe(
        map((user: LoggedInUser) => {
          if (user) {
            sessionStorage.removeItem('email');
            this._isVerifyingAccountSig.set(false);
            this.setCurrentUser(user);
            this.setGetReturnUrl(); // Never put it in the setCurrentUser() or all pages refreshes land on members only.
            this.showLoginSuccessMessage(user.userName);
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
            sessionStorage.removeItem('csrfToken'); // Clean after login to generate new token using csrfInterceptor

            if (user.isEmailNotConfirmed) {
              sessionStorage.setItem('email', user.email);
              this._isVerifyingAccountSig.set(true);
            } else {
              this._isVerifyingAccountSig.set(false);
              this.setCurrentUser(user);
              this.setGetReturnUrl(); // Never put it in the setCurrentUser() or all pages refreshes land on members only.
              this.showLoginSuccessMessage(user.userName);
              return user;
            }
          }
          return null;
        })
      );
  }

  refreshTokens(): Observable<void> {
    return this._http.get<void>(this.baseUrl + 'refresh-tokens');
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
    return this._http.post<ApiResponseMessage>(this.baseUrl + 'request-reset-password', request);
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
    sessionStorage.clear();
    this.loggedInUserSig.set(undefined);
    this._router.navigate(['account']);
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
    this.loggedInUserSig.set(loggedInUser);
    this._presenceService.createHubConnection();
    // Set it to false to show Hallboard in color to the loggedInUser. Default was true
    this._responsiveService.isWelcomeCompSig.set(false);
  }

  setGetReturnUrl(): void {
    const returnUrl: string | null = localStorage.getItem('returnUrl');

    this.loggedInUserSig()?.rolesStr?.includes(Roles.ADMIN)
      ? this._router.navigate(['/admin'])
      : returnUrl
        ? this._router.navigate([returnUrl])
        : this._router.navigate(['/main']);

    localStorage.removeItem('returnUrl');
  }

  private showLoginSuccessMessage(userName: string): void {
    this._snackBar.open("You are logged in as: " + userName, "Close", {
      verticalPosition: 'bottom',
      horizontalPosition: 'center',
      duration: 7000
    })
  }
}
