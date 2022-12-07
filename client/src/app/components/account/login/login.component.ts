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

  user$!: Observable<UserProfile>;
  currentUser$!: Observable<UserProfile | null>;

  constructor(private authService: AccountService, private fb: FormBuilder) {

  }

  ngOnInit(): void {
    this.loginFg;
    // this.currentUser$ = this.authService.currentUser$;
  }


  loginFg = this.fb.group({
    emailCtrl: ['', [Validators.required, Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', Validators.required]
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

    this.user$ = this.authService.login(userLoginInput);

    this.user$.subscribe(res => console.log(res.email));
  }

  logout(): void {
    this.authService.logout();
  }

}
