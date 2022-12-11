import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { UserLogin } from 'src/app/_models/account/user-login.model';
import { UserProfile } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  user$!: Observable<UserProfile | null>;
  subscrition!: Subscription;

  constructor(private authService: AccountService, private fb: FormBuilder, private router: Router) { }

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
          this.router.navigate(['/'])
          console.log('User:', res)
        },
        error: err => console.log('Login error:', err),
        complete: () => console.log('Login successful.')
      });
    
    this.loginFg.markAllAsTouched();
  }
}
