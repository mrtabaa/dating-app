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
    emailCtrl: ['', {
      validators: [
        Validators.required,
        Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)
      ],
      validatorsAsync: []
    }, { updateOn: "blur" }],
    // passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]]
    passwordCtrl: ['', []]
  });
  //#endregion

  //#region Forms Properties
  // Lab's Info
  get EmailCtrl(): FormControl {
    return this.registerFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
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
    console.log(this.EmailCtrl);
  }
  //#endregion
}
