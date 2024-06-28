import { Component, OnDestroy, inject, Renderer2, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription, take } from 'rxjs';
import { UserLogin } from '../../../models/account/user-login.model';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { AccountService } from '../../../services/account.service';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Router, RouterLink } from '@angular/router';
import { MatDivider } from '@angular/material/divider';
import { UserRegister } from '../../../models/account/user-register.model';
import { ResponsiveService } from '../../../services/responsive.service';
import { RecaptchaV3Module, ReCaptchaV3Service } from "ng-recaptcha";
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    InputCvaComponent, RouterLink,
    FormsModule, ReactiveFormsModule,
    RecaptchaV3Module,
    MatButtonModule, MatInputModule, MatCheckboxModule, MatDivider, MatSlideToggleModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit, OnDestroy {
  private accountService = inject(AccountService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  router = inject(Router);
  renderer = inject(Renderer2);
  private _recaptchaService = inject(ReCaptchaV3Service);
  private _recaptchaToken: string | undefined;

  user$: Observable<LoggedInUser | null> | undefined;
  private _subscribedLogin: Subscription | undefined;
  private _subscribedRecaptcha: Subscription | undefined;

  ngOnInit(): void {
    this.validateRecaptcha();
  }

  ngOnDestroy(): void {
    this._subscribedLogin?.unsubscribe();
    this._subscribedRecaptcha?.unsubscribe();
  }

  loginFg = this.fb.group({
    emailUsernameCtrl: ['', [Validators.required, Validators.maxLength(50)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(50), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/)]],
    recaptchaCtrl: [false, [Validators.required]],
    rememberMeCtrl: [false, []]
  });

  get EmailUsernameCtrl(): FormControl {
    return this.loginFg.get('emailUsernameCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.loginFg.get('passwordCtrl') as FormControl;
  }
  get RecaptchaCtrl(): FormControl {
    return this.loginFg.get('recaptchaCtrl') as FormControl;
  }
  get RememberMeCtrl(): FormControl {
    return this.loginFg.get('rememberMeCtrl') as FormControl;
  }

  validateRecaptcha(): void {
    this._subscribedRecaptcha = this._recaptchaService.execute('login').subscribe(
      (token: string) => this._recaptchaToken = token);
  }

  loginEmailUsername(): void {
    if (this._recaptchaToken) {
      const userLoginInput: UserLogin = {
        emailUsername: this.EmailUsernameCtrl.value,
        password: this.PasswordCtrl.value,
        recaptchaToken: this._recaptchaToken
      };

      this._subscribedLogin = this.accountService.login(userLoginInput)
        .subscribe({
          next: res => {
            this.snackBar.open('You logged in as: ' + res?.userName, 'Close', { verticalPosition: 'bottom', horizontalPosition: 'center', duration: 7000 })
          }
          // complete: () => console.log('Login successful.')
        });

      this.loginFg.markAllAsTouched();
    }
  }

  enterAdminCreds(): void {
    this.EmailUsernameCtrl.setValue('admin@a.com');
    this.PasswordCtrl.setValue('Aaaaaaa1')
  }

  generateMemberCreds(): void {
    const randomAccount = 'ab' + this.generateRandomText(3);

    if (this._recaptchaToken) {

      const userRegInput: UserRegister = {
        email: randomAccount + '@a.com',
        username: randomAccount,
        password: 'Aaaaaaa1',
        confirmPassword: 'Aaaaaaa1',
        dateOfBirth: '2000-01-01',
        gender: 'male',
        recaptchaToken: this._recaptchaToken
      }

      this.accountService.register(userRegInput)
        .pipe(
          take(1)
        ).subscribe({
          next: (response: LoggedInUser | null) => {
            if (response) {
              this.EmailUsernameCtrl.setValue(randomAccount);
              this.PasswordCtrl.setValue('Aaaaaaa1');
              this._recaptchaToken = response.recaptchaToken;
            }
          }
        });
    }
  }

  private generateRandomText(length: number): string {
    const characters = '0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
      result += characters.charAt(Math.floor(Math.random() * characters.length));
    }
    return result;
  }
}