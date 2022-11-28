import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { UserLogin } from 'src/app/_models/user-login.model';
import { UserProfile } from 'src/app/_models/user.model';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {


  userCredInput: UserLogin = { email: null, password: null };
  user$!: Observable<void>;
  currentUser$!: Observable<UserProfile | null>;

  constructor(private authService: AuthService, private fb: FormBuilder) {

  }

  ngOnInit(): void {
    this.currentUser$ = this.authService.currentUser$;
  }


  emailLoginFg = this.fb.group({
    emailCtrl: ['', [Validators.required, Validators.pattern(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/)]],
    passwordCtrl: ['', Validators.required]
  });


  get EmailCtrl(): AbstractControl {
    return this.emailLoginFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): AbstractControl {
    return this.emailLoginFg.get('passwordCtrl') as FormControl;
  }

  loginEmail(): void {
    this.userCredInput.email = this.EmailCtrl.value;
    this.userCredInput.password = this.PasswordCtrl.value;
    this.user$ = this.authService.login(this.userCredInput);
    console.log(this.emailLoginFg);
  }

  loginGoogle(): void {
    // this.authService.login(new firebase.default.auth.GoogleAuthProvider());
  }

  loginTwitter(): void {
    // this.authService.login(new firebase.default.auth.FacebookAuthProvider());
  }

  logout(): void {
    this.authService.logout();
  }

}
