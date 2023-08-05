import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControlOptions, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserRegister } from 'src/app/models/account/user-register.model';
import { AccountService } from 'src/app/services/account.service';
import { RegisterValidators } from '../../helpers/validators/register.validator';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit, OnDestroy {
  minDate = new Date();
  maxDate = new Date();
  isEmailExist: string | undefined;

  subscriptionRegisterUser!: Subscription;

  //#region Base
  constructor(private fb: FormBuilder, private accountService: AccountService, private router: Router) { }

  ngOnInit() {
    this.registerFg;

    // set datePicker year limitations
    const currentYear = new Date().getFullYear();
    this.minDate = new Date(currentYear - 99, 0, 1); // not older than 99 years
    this.maxDate = new Date(currentYear - 18, 0, 1); // not earlier than 18 years
  }

  ngOnDestroy(): void {
    if (this.subscriptionRegisterUser)
      this.subscriptionRegisterUser.unsubscribe();
  }
  //#endregion

  //#region Forms Group/controler
  registerFg = this.fb.group({
    emailCtrl: ['', [Validators.required, Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    confirmPasswordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    dateOfBirthCtrl: ['', [Validators.required]],
    knownAsCtrl: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(30)]],
    genderCtrl: ['female', [Validators.required]],
    cityCtrl: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
    countryCtrl: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
  }, { validators: [RegisterValidators.confirmPassword] } as AbstractControlOptions);
  //#endregion

  //#region Forms Properties
  // Lab's Info
  get EmailCtrl(): FormControl {
    return this.registerFg.get('emailCtrl') as FormControl;
  }
  get PasswordCtrl(): FormControl {
    return this.registerFg.get('passwordCtrl') as FormControl;
  }
  get ConfirmPasswordCtrl(): FormControl {
    return this.registerFg.get('confirmPasswordCtrl') as FormControl;
  }
  get DateOfBirthCtrl(): FormControl {
    return this.registerFg.get('dateOfBirthCtrl') as FormControl;
  }
  get KnownAsCtrl(): FormControl {
    return this.registerFg.get('knownAsCtrl') as FormControl;
  }
  get GenderCtrl(): FormControl {
    return this.registerFg.get('genderCtrl') as FormControl;
  }
  get CityCtrl(): FormControl {
    return this.registerFg.get('cityCtrl') as FormControl;
  }
  get CountryCtrl(): FormControl {
    return this.registerFg.get('countryCtrl') as FormControl;
  }
  //#endregion


  //#region Methods
  registerUser(): void {
    const dob = this.getDateOnly(this.DateOfBirthCtrl.value);

    let userRegisterInput: UserRegister = {
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value,
      confirmPassword: this.ConfirmPasswordCtrl.value,
      dateOfBirth: dob,
      knownAs: this.KnownAsCtrl.value,
      gender: this.GenderCtrl.value,
      city: this.CityCtrl.value,
      country: this.CountryCtrl.value
    };

    this.subscriptionRegisterUser = this.accountService.register(userRegisterInput)
      .subscribe({
        next: res => {
          this.router.navigate(['/members']);
          console.log(res);
        },
        error: err => this.isEmailExist = err.error,
        complete: () => console.log('Register successful.')
      });

    // to show validation mat-error on submit button (also added to DOM mat-error)
    this.registerFg.markAllAsTouched();
  }

  private getDateOnly(dob: string | null): string | undefined {
    if (!dob) return undefined;

    let theDob = new Date(dob);
    return new Date(theDob.setMinutes(theDob.getMinutes() - theDob.getTimezoneOffset())).toISOString().slice(0, 10);
  }

  // other methods
  checkStatus(): void {
    console.log(this.registerFg.value);
  }
  //#endregion
}
