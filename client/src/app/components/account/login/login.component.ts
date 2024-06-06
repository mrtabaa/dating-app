import { Component, Input, OnDestroy, OnInit, inject, Renderer2 } from '@angular/core';
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
import { NgxTurnstileModule, NgxTurnstileFormsModule } from "ngx-turnstile"; // CloudFlare
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    InputCvaComponent, RouterLink,
    FormsModule, ReactiveFormsModule,
    NgxTurnstileModule, NgxTurnstileFormsModule,
    MatButtonModule, MatInputModule, MatCheckboxModule, MatDivider
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit, OnDestroy {
  private accountService = inject(AccountService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  router = inject(Router);
  renderer = inject(Renderer2);

  @Input() isLoginShownIn = false;

  user$!: Observable<LoggedInUser | null>;
  subscrition!: Subscription;
  turnsTileSiteKey = environment.turnstileSiteKey;
  isTurnstileActive = false;

  ngOnInit(): void {
    this.turnsTileChange();
  }

  ngOnDestroy(): void {
    if (this.subscrition)
      this.subscrition.unsubscribe();
  }

  turnsTileChange(): void {
    this.renderer.listen('window', 'message', (event) => {
      if (event.data.event !== 'init') {
        return;
      }

      const turnstileIframe = this.renderer.selectRootElement(`#cf-chl-widget-${event.data.widgetId}`);
      if (!turnstileIframe) {
        return;
      }

      this.renderer.setStyle(turnstileIframe, 'width', '100%');
      this.renderer.setStyle(turnstileIframe, 'height', '65px');
      this.renderer.setStyle(turnstileIframe, 'display', 'flex'); // Changed to 'block' to ensure visibility
      event.stopImmediatePropagation();
    });
  }

  loginFg = this.fb.group({
    emailUsernameCtrl: ['', [Validators.required, Validators.maxLength(50)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(50), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/)]],
    rememberMeCtrl: [false, []],
    turnsTileCtrl: [null, [Validators.required]]
  });

  get EmailUsernameCtrl(): FormControl {
    return this.loginFg.get('emailUsernameCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.loginFg.get('passwordCtrl') as FormControl;
  }
  get RememberMeCtrl(): FormControl {
    return this.loginFg.get('rememberMeCtrl') as FormControl;
  }
  get TurnsTileCtrl(): FormControl {
    return this.loginFg.get('turnsTileCtrl') as FormControl;
  }

  loginEmailUsername(): void {
    const userLoginInput: UserLogin = {
      emailUsername: this.EmailUsernameCtrl.value,
      password: this.PasswordCtrl.value,
      turnsTileToken: this.TurnsTileCtrl.value
    };

    this.subscrition = this.accountService.login(userLoginInput)
      .subscribe({
        next: res => {
          this.snackBar.open('You logged in as: ' + res?.userName, 'Close', { verticalPosition: 'bottom', horizontalPosition: 'center', duration: 7000 })
        }
        // complete: () => console.log('Login successful.')
      });

    this.loginFg.markAllAsTouched();
  }

  enterAdminCreds(): void {
    this.EmailUsernameCtrl.setValue('admin@a.com');
    this.PasswordCtrl.setValue('Aaaaaaa1')
  }

  generateMemberCreds(): void {
    const randomAccount = 'ab' + this.generateRandomText(3);

    const userRegInput: UserRegister = {
      email: randomAccount + '@a.com',
      username: randomAccount,
      password: 'Aaaaaaa1',
      confirmPassword: 'Aaaaaaa1',
      dateOfBirth: '2000-01-01',
      gender: 'male',
      turnsTileToken: this.TurnsTileCtrl.value
    }

    this.accountService.register(userRegInput)
      .pipe(
        take(1)
      ).subscribe({
        next: response => {
          if (response) {
            this.EmailUsernameCtrl.setValue(randomAccount);
            this.PasswordCtrl.setValue('Aaaaaaa1')
          }
        }
      });
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
