import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, Validators } from '@angular/forms';
import { UserRegister } from 'src/app/_models/account/user-register.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {

  //#region Base
  constructor(private fb: FormBuilder, private accountService: AccountService) { }

  ngOnInit() {
    this.registerFg;
  }
  //#endregion

  //#region Forms Group/controler
  registerFg = this.fb.group({
    emailCtrl: ['', [
      // Validators.required,
      // Validators.pattern(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/)
    ]],
    // passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]]
    passwordCtrl: ['', []]
  });
  //#endregion

  //#region Forms Properties
  // Lab's Info
  get EmailCtrl(): AbstractControl {
    return this.registerFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): AbstractControl {
    return this.registerFg.get('passwordCtrl') as FormControl;
  }
  //#endregion


  //#region Methods
  registerUser() {
    let userRegister: UserRegister = {
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value
    };

    this.accountService.register(userRegister);
  }

  // other methods
  checkStatus(): void {
    console.log(this.registerFg.value);
  }
  //#endregion
}
