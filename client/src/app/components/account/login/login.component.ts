import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription } from 'rxjs';
import { UserLogin } from 'src/app/models/account/user-login.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  user$!: Observable<User | null>;
  subscrition!: Subscription;

  constructor(
    private authService: AccountService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar) { }

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

    this.subscrition = this.authService.login(userLoginInput)
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
