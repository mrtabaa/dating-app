import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserRegister } from 'src/app/models/account/user-register.model';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit, OnDestroy {

  subscriptionRegisterUser!: Subscription;

  //#region Base
  constructor(private fb: FormBuilder, private accountService: AccountService, private router: Router) { }

  ngOnInit() {
    this.registerFg;
  }

  ngOnDestroy(): void {
    if (this.subscriptionRegisterUser)
      this.subscriptionRegisterUser.unsubscribe();
  }
  //#endregion

  //#region Forms Group/controler
  registerFg = this.fb.group({
    nameCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    emailCtrl: ['', [Validators.required, Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    confirmPasswordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    dateOfBirthCtrl: [''],
    knownAsCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    genderCtrl: ['', [Validators.required]],
    introductionCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    lookingForCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    interestsCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    cityCtrl: [''],
    countryCtrl: [''],
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
  get IntroductionCtrl(): FormControl {
    return this.registerFg.get('introductionCtrl') as FormControl;
  }
  get LookingForCtrl(): FormControl {
    return this.registerFg.get('lookingForCtrl') as FormControl;
  }
  get InterestsCtrl(): FormControl {
    return this.registerFg.get('interestsCtrl') as FormControl;
  }
  get CityCtrl(): FormControl {
    return this.registerFg.get('cityCtrl') as FormControl;
  }
  get CountryCtrl(): FormControl {
    return this.registerFg.get('countryCtrl') as FormControl; 
  }
  //#endregion


  //#region Methods
  registerUser() {
    let userRegisterInput: UserRegister = {
      name: this.NameCtrl.value,
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value,
      confirmPassword: this.ConfirmPasswordCtrl.value,
      dateOfBirth: this.DateOfBirthCtrl.value,
      knownAs: this.KnownAsCtrl.value,
      gender: this.GenderCtrl.value,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingForCtrl.value,
      interests: this.InterestsCtrl.value,
      city: this.CityCtrl.value,
      country: this.CountryCtrl.value
    };

    this.subscriptionRegisterUser = this.accountService.register(userRegisterInput)
      .subscribe({
        next: res => {
          this.router.navigate(['/members']);
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
