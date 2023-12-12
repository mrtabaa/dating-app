import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription } from 'rxjs';
import { UserLogin } from '../../../models/account/user-login.model';
import { LoggedInUser } from '../../../models/loggedInUser.model';
import { AccountService } from '../../../services/account.service';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    InputCvaComponent,
    FormsModule, ReactiveFormsModule,
    MatButtonModule, MatInputModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit, OnDestroy {
  private accountService = inject(AccountService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

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
    emailCtrl: ['', [Validators.required, Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]]
  });


  get EmailCtrl(): FormControl {
    return this.loginFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.loginFg.get('passwordCtrl') as FormControl;
  }

  loginEmail(): void {
    let userLoginInput: UserLogin = {
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value
    };

    this.subscrition = this.accountService.login(userLoginInput)
      .subscribe({
        next: res => {
          console.log('User:', res)
        },
        error: err => this.snackBar.open(err.error, "Close", {
          horizontalPosition: 'end', verticalPosition: 'bottom', duration: 7000
        }),
        // complete: () => console.log('Login successful.')
      });

    this.loginFg.markAllAsTouched();
  }
}
