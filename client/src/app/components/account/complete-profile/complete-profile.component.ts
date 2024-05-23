import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';
import { MatExpansionModule } from '@angular/material/expansion';
import { Router, RouterLink } from '@angular/router';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';
import { PhotoEditorComponent } from '../../user/photo-editor/photo-editor.component';
import { MemberService } from '../../../services/member.service';
import { Observable, map, startWith, take } from 'rxjs';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { Member } from '../../../models/member.model';
import { CommonModule } from '@angular/common';
import { UserUpdate } from '../../../models/user-update.model';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { UserService } from '../../../services/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AccountService } from '../../../services/account.service';
import { STEPPER_GLOBAL_OPTIONS, StepperOrientation } from '@angular/cdk/stepper';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Country } from '../../../models/country.model';
import { CountryService } from '../../../services/country.service';
import { ClearControlFieldByClickDirective } from '../../../directives/clear-control-field-by-click.directive';
import { CheckCountryExistsDirective } from '../../../directives/check-country-exists.directive';
import { MatAutocompleteModule } from '@angular/material/autocomplete';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [
    FormsModule, ReactiveFormsModule, CommonModule,
    PhotoEditorComponent,
    MatStepperModule, InputCvaComponent, RouterLink, MatButtonModule, MatInputModule, MatExpansionModule, MatAutocompleteModule,
    ClearControlFieldByClickDirective, CheckCountryExistsDirective
  ],
  templateUrl: './complete-profile.component.html',
  styleUrl: './complete-profile.component.scss',
  providers: [{
    provide: STEPPER_GLOBAL_OPTIONS, useValue: { displayDefaultIndicatorType: false }
  }]
})
export class CompleteProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private memberService = inject(MemberService);
  private userService = inject(UserService);
  private accountService = inject(AccountService);
  private countryService = inject(CountryService);
  private router = inject(Router);
  private matSnack = inject(MatSnackBar);
  private breakpointObserver = inject(BreakpointObserver);
  stepperOrientation: Observable<StepperOrientation>;

  member: Member | undefined;
  readonly maxTextAreaChars: number = 1000;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 30;
  readonly starterLable = 'Starter';
  readonly starterSummary = 'Your basic information';
  readonly photosLabel = 'Photos';
  readonly introductionLabel = 'Introduction';
  readonly interestsLabel = 'Interests';
  readonly lookingForLabel = 'Looking for';
  panelOpenState = false;
  filteredCountries$!: Observable<Country[]>;

  constructor() {
    this.stepperOrientation = this.breakpointObserver.observe('(min-width: 990px)')
      .pipe(map(({ matches }) => matches ? 'horizontal' : 'vertical'));
  }

  ngOnInit(): void {
    this.getMember();

    // filter country with user input
    this.filterCountries();

    // when no country is selected
    this.hideCountryFlag();
  }

  getMember(): void {
    const loggedInUserStr = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

      this.memberService.getMemberByUsername(loggedInUser.userName)?.pipe(take(1)).subscribe(member => {
        if (member) {
          this.member = member;
        }
      });
    }
  }

  starterFg = this.fb.group({
    knownAsCtrl: [null, [Validators.required, Validators.minLength(1), Validators.maxLength(50)]],
    selectedCountryCtrl: ['', Validators.required],
    countryCtrl: [null, [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]],
    stateCtrl: ['', [Validators.required, Validators.maxLength(20)]],
    cityCtrl: [null, [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]]
  })
  introductionCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  interestsCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  lookingForCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  photosCtrl = this.fb.control(null);
  // photosCtrl = this.fb.control(null, [Validators.required]);

  get KnownAsCtrl(): AbstractControl {
    return this.starterFg.get('knownAsCtrl') as FormControl;
  }
  get SelectedCountryCtrl(): AbstractControl {
    return this.starterFg.get('selectedCountryCtrl') as FormControl;
  }
  get CountryCtrl(): AbstractControl {
    return this.starterFg.get('countryCtrl') as FormControl;
  }
  get StateCtrl(): AbstractControl {
    return this.starterFg.get('stateCtrl') as FormControl;
  }
  get CityCtrl(): AbstractControl {
    return this.starterFg.get('cityCtrl') as FormControl;
  }
  get IntroductionCtrl(): AbstractControl {
    return this.introductionCtrl as FormControl;
  }
  get InterestsCtrl(): AbstractControl {
    return this.interestsCtrl as FormControl;
  }
  get LookingForCtrl(): AbstractControl {
    return this.lookingForCtrl as FormControl;
  }
  get PhotosCtrl(): AbstractControl {
    return this.photosCtrl as FormControl;
  }

  submit(): void {
    const updatedUser: UserUpdate = {
      knownAs: this.KnownAsCtrl.value,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingForCtrl.value,
      interests: this.InterestsCtrl.value,
      country: this.CountryCtrl.value,
      state: this.StateCtrl.value,
      city: this.CityCtrl.value
    }

    this.userService.updateUser(updatedUser)
      .pipe(take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          if (response.message) {
            const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');
            if (loggedInUserStr) {
              const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

              loggedInUser.isProfileCompleted = true;

              this.router.navigate(['members']);

              this.accountService.setCurrentUser(loggedInUser);
            }

            this.matSnack.open(response.message, "Close", { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000 });
          }
        }
      });
  }

  // ngOnInit
  filterCountries(): void {
    this.filteredCountries$ = this.CountryCtrl.valueChanges
      .pipe(
        startWith(''),
        map((value: string) => this.countryService.filterCountries(value))
      );
  }

  // if no country is selected
  // ngOnInit
  hideCountryFlag(): void {
    this.CountryCtrl.valueChanges.subscribe((value: string) => {
      if (value.length < 2) {
        this.SelectedCountryCtrl.setErrors({ 'invalid': true });
      }
    })
  }

  // get from DOM (onSelectionChange)
  getSelectedCountry(country: Country, event: any): void {
    if (event.isUserInput) {  // check if the option is selected
      //set phoneNumber
      this.SelectedCountryCtrl.setValue(country);
    }
  }

  moveFocusToNext(leaveFocus: HTMLInputElement, gainFocus: HTMLInputElement): void {
    setTimeout(() => {
      leaveFocus.blur();
      gainFocus.focus();
    });
  }
}
