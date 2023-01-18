import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserRegister } from 'src/app/_models/account/user-register.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit, OnDestroy {

  subscription!: Subscription;

  //#region Base
  constructor(private fb: FormBuilder, private accountService: AccountService, private router: Router) { }

  ngOnInit() {
    this.registerFg;
  }

  ngOnDestroy(): void {
    if (this.subscription)
      this.subscription.unsubscribe();
  }
  //#endregion

  //#region Forms Group/controler
  registerFg = this.fb.group({
    nameCtrl: ['', Validators.required, Validators.minLength(7), Validators.maxLength(20)],
    emailCtrl: ['', [Validators.required, Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]]
  });
  //#endregion

  //#region Forms Properties
  // Lab's Info
  get NameCtrl(): FormControl {
    return this.registerFg.get('nameCtrl') as FormControl;
  }
  get EmailCtrl(): FormControl {
    return this.registerFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.registerFg.get('passwordCtrl') as FormControl;
  }
  //#endregion


  //#region Methods
  registerUser() {
    let userRegisterInput: UserRegister = {
      name: this.NameCtrl.value,
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value
    };

    this.subscription = this.accountService.register(userRegisterInput)
      .subscribe({
        next: res => {
          this.router.navigate(['/']);
          console.log(res);
        },
        error: err => console.log('Register error:', err),
        complete: () => console.log('Register successful.')
      });

    // to show validation mat-error on submit button (also added to DOM mat-error)
    this.registerFg.markAllAsTouched();
  }

  // other methods
  checkStatus(): void {
    console.log(this.PasswordCtrl);
  }
  //#endregion
}
