import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { UserLogin } from 'src/app/_models/account/user-login.model';
import { UserProfile } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {


  userLoginInput: UserLogin = { email: null, password: null };
  user$!: Observable<void>;
  currentUser$!: Observable<UserProfile | null>;

  constructor(private authService: AccountService, private fb: FormBuilder) {

  }

  ngOnInit(): void {
    this.currentUser$ = this.authService.currentUser$;
  }


  loginFg = this.fb.group({
    emailCtrl: ['', [Validators.required, Validators.pattern(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/)]],
    passwordCtrl: ['', Validators.required]
  });


  get EmailCtrl(): FormControl {
    return this.loginFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.loginFg.get('passwordCtrl') as FormControl;
  }

  loginEmail(): void {
    this.userLoginInput.email = this.EmailCtrl.value;
    this.userLoginInput.password = this.PasswordCtrl.value;
    this.user$ = this.authService.login(this.userLoginInput);
    console.log(this.loginFg);
  }

  logout(): void {
    this.authService.logout();
  }

}
