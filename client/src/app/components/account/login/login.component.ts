import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription } from 'rxjs';
import { UserLogin } from '../../../models/account/user-login.model';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { AccountService } from '../../../services/account.service';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Router, RouterLink } from '@angular/router';
import { MatDivider } from '@angular/material/divider';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    InputCvaComponent, RouterLink,
    FormsModule, ReactiveFormsModule,
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

  user$!: Observable<LoggedInUser | null>;
  subscrition!: Subscription;

  ngOnInit(): void {
    this.loginFg;
  }

  ngOnDestroy(): void {
    if (this.subscrition)
      this.subscrition.unsubscribe();
  }

  loginFg = this.fb.group({
    emailUsernameCtrl: ['', [Validators.required, Validators.maxLength(50)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(50), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/)]],
    rememberMeCtrl: [false, []]
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

  loginEmailUsername(): void {
    const userLoginInput: UserLogin = {
      emailUsername: this.EmailUsernameCtrl.value,
      password: this.PasswordCtrl.value
    };

    this.subscrition = this.accountService.login(userLoginInput)
      .subscribe({
        next: res => {
          this.snackBar.open("You logged in as: " + res?.userName, "Close", { verticalPosition: 'bottom', horizontalPosition: 'center', duration: 7000 })
        }
        // complete: () => console.log('Login successful.')
      });

    this.loginFg.markAllAsTouched();
  }

  enterAdminCreds(): void {
    this.EmailUsernameCtrl.setValue('admin@a.com');
    this.PasswordCtrl.setValue('Aaaaaaa1')
  }
  enterMemberCreds(): void {
    this.EmailUsernameCtrl.setValue('a');
    this.PasswordCtrl.setValue('Aaaaaaa1')
  }
}
