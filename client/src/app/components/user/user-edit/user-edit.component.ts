import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Subscription, take } from 'rxjs';
import { UserUpdate } from '../../../models/user-update.model';
import { Member } from '../../../models/member.model';
import { PhotoEditorComponent } from '../../user/photo-editor/photo-editor.component';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MemberService } from '../../../services/member.service';
import { UserService } from '../../../services/user.service';
import { IntlModule } from 'angular-ecmascript-intl';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { AccountService } from '../../../services/account.service';
import { DatePickerCvaComponent } from '../../_helpers/date-picker-cva/date-picker-cva.component';
import { GooglePlacesComponent } from '../../google-places/google-places.component';
import { GooglePlacesService } from '../../../services/google-places.service';
import { CommonService } from '../../../services/common.service';
import { ResponsiveService } from '../../../services/responsive.service';
import { LoadingService } from '../../../services/loading.service';
import { RecaptchaV3Module, ReCaptchaV3Service } from "ng-recaptcha";
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, FormsModule, ReactiveFormsModule,
    PhotoEditorComponent, DatePickerCvaComponent, GooglePlacesComponent,
    RecaptchaV3Module,
    MatCardModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSlideToggleModule,
    IntlModule
  ],
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.scss']
})
export class UserEditComponent implements OnInit, OnDestroy {
  private _userService = inject(UserService);
  private _memberService = inject(MemberService);
  private _accountService = inject(AccountService);
  private _googlePlacesService = inject(GooglePlacesService);
  private _commonService = inject(CommonService);
  private _fb = inject(FormBuilder);
  private _matSnak = inject(MatSnackBar);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;
  private _recaptchaService = inject(ReCaptchaV3Service);
  private _recaptchaToken: string | undefined;

  private _userEditFgSubscribed: Subscription | undefined;
  private _subscribedRecaptcha: Subscription | undefined;

  member: Member | undefined;

  minDate = new Date();
  maxDate = new Date();
  readonly minTextAreaChars: number = 10;
  readonly maxTextAreaChars: number = 1000;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 30;

  ngOnInit(): void {
    // set datePicker year limitations
    const currentYear = new Date().getFullYear();
    this.minDate = new Date(currentYear - 99, 0, 1); // not older than 99 years
    this.maxDate = new Date(currentYear - 18, 0, 1); // not earlier than 18 years

    this.getMember();

    this.validateRecaptcha();
  }

  ngOnDestroy(): void {
    if (this._userEditFgSubscribed)
      this._userEditFgSubscribed.unsubscribe();

    this._subscribedRecaptcha?.unsubscribe();
  }

  getMember(): void {
    const loggedInUser: LoggedInUser | null = this.getLoggedInUserLocalStorage();

    if (loggedInUser)
      this._memberService.getMemberByUsername(loggedInUser.userName)?.pipe(take(1)).subscribe(member => {
        if (member) {
          this.member = member;

          this.initContollersValues(member);
        }
      });
  }

  userEditFg: FormGroup = this._fb.group({
    knownAsCtrl: [null, [Validators.required, Validators.minLength(1), Validators.maxLength(50)]],
    dateOfBirthCtrl: [null, [Validators.required]],
    introductionCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]],
    lookingForCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]],
    interestsCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]]
  });

  recaptchaCtrl: FormControl = this._fb.control(false, [Validators.required]);

  get KnownAsCtrl(): AbstractControl {
    return this.userEditFg.get('knownAsCtrl') as FormControl;
  }
  get DateOfBirthCtrl(): FormControl {
    return this.userEditFg.get('dateOfBirthCtrl') as FormControl;
  }
  get IntroductionCtrl(): AbstractControl {
    return this.userEditFg.get('introductionCtrl') as FormControl;
  }
  get LookingForCtrl(): AbstractControl {
    return this.userEditFg.get('lookingForCtrl') as FormControl;
  }
  get InterestsCtrl(): AbstractControl {
    return this.userEditFg.get('interestsCtrl') as FormControl;
  }

  initContollersValues(member: Member) {
    this.KnownAsCtrl.setValue(member.knownAs);
    this.DateOfBirthCtrl.setValue(member.dateOfBirth);
    this.IntroductionCtrl.setValue(member.introduction);
    this.LookingForCtrl.setValue(member.lookingFor);
    this.InterestsCtrl.setValue(member.interests);
    this._googlePlacesService.isCountrySelectedSig.set(true);
    this._googlePlacesService.countryAcrSig.set(member.countryAcr);
    this._googlePlacesService.countrySig.set(member.country);
    this._googlePlacesService.stateSig.set(member.state);
    this._googlePlacesService.citySig.set(member.city);
  }

  updateMemberAfterUpdateSucceed(): void {
    if (this.member) {
      this.member.knownAs = this.KnownAsCtrl.value;
      this.member.dateOfBirth = this.DateOfBirthCtrl.value;
      this.member.introduction = this.IntroductionCtrl.value;
      this.member.lookingFor = this.LookingForCtrl.value;
      this.member.interests = this.InterestsCtrl.value;
      this.member.countryAcr = this._googlePlacesService.countryAcrSig() as string;
      this.member.country = this._googlePlacesService.countrySig() as string;
      this.member.state = this._googlePlacesService.stateSig() as string;
      this.member.city = this._googlePlacesService.citySig() as string;
    }
  }

  disableItemsOnNoChangeValues(): boolean {
    if (
      this.member
      && this.member.knownAs === this.KnownAsCtrl.value
      && this.member.dateOfBirth === this.DateOfBirthCtrl.value
      && this.member.introduction === this.IntroductionCtrl.value
      && this.member.lookingFor === this.LookingForCtrl.value
      && this.member.interests === this.InterestsCtrl.value
      && this.member.countryAcr === this._googlePlacesService.countryAcrSig()
      && this.member.country === this._googlePlacesService.countrySig()
      && this.member.state === this._googlePlacesService.stateSig()
      && this.member.city === this._googlePlacesService.citySig()
    ) {

      this._commonService.isPreventingLeavingPage = false

      return true;
    }

    this._commonService.isPreventingLeavingPage = true;

    return false;
  }

  validateRecaptcha(): void {
    this._subscribedRecaptcha = this._recaptchaService.execute('edit').subscribe(
      (token: string) => this._recaptchaToken = token);
  }

  updateUser(member: Member): void {
    if (member && this._recaptchaToken) {
      const updatedUser: UserUpdate = {
        username: member.userName,
        knownAs: this.KnownAsCtrl.value,
        dateOfBirth: this.DateOfBirthCtrl.value,
        introduction: this.IntroductionCtrl.value,
        lookingFor: this.LookingForCtrl.value,
        interests: this.InterestsCtrl.value,
        countryAcr: this._googlePlacesService.countryAcrSig() as string,
        country: this._googlePlacesService.countrySig() as string,
        state: this._googlePlacesService.stateSig() as string,
        city: this._googlePlacesService.citySig() as string,
        isProfileCompleted: true
      }

      this._userService.updateUser(updatedUser)
        .pipe(take(1))
        .subscribe({
          next: (response: ApiResponseMessage) => {
            if (response.message) {
              this._matSnak.open(response.message, "Close", { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000 });

              const loggedInUser: LoggedInUser | null = this.getLoggedInUserLocalStorage();

              if (loggedInUser) {
                loggedInUser.knownAs = this.KnownAsCtrl.value;

                this._accountService.setCurrentUser(loggedInUser);

                this.updateMemberAfterUpdateSucceed();
              }
            }
          }
        });

      this.userEditFg.markAsPristine();
    }
  }

  getLoggedInUserLocalStorage(): LoggedInUser | null {
    const loggedInUserStr = localStorage.getItem('loggedInUser');

    if (loggedInUserStr)
      return JSON.parse(loggedInUserStr);

    return null;
  }

  logForm() {
    console.log(this.IntroductionCtrl)
  }
}
