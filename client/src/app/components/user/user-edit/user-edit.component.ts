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

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, FormsModule, ReactiveFormsModule,
    PhotoEditorComponent, DatePickerCvaComponent, GooglePlacesComponent,
    MatCardModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    IntlModule
  ],
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.scss']
})
export class UserEditComponent implements OnInit, OnDestroy {
  private userService = inject(UserService);
  private memberService = inject(MemberService);
  private accountService = inject(AccountService);
  private googlePlacesService = inject(GooglePlacesService);
  private commonService = inject(CommonService);
  private fb = inject(FormBuilder);
  private matSnak = inject(MatSnackBar);

  private userEditFgSubscribed: Subscription | undefined;

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
  }

  ngOnDestroy(): void {
    if (this.userEditFgSubscribed)
      this.userEditFgSubscribed.unsubscribe();
  }

  getMember(): void {
    const loggedInUser: LoggedInUser | null = this.getLoggedInUserLocalStorage();

    if (loggedInUser)
      this.memberService.getMemberByUsername(loggedInUser.userName)?.pipe(take(1)).subscribe(member => {
        if (member) {
          this.member = member;

          this.initContollersValues(member);
        }
      });
  }

  userEditFg: FormGroup = this.fb.group({
    knownAsCtrl: [null, [Validators.required, Validators.minLength(1), Validators.maxLength(50)]],
    dateOfBirthCtrl: [null, [Validators.required]],
    introductionCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]],
    lookingForCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]],
    interestsCtrl: ['', [Validators.maxLength(this.maxTextAreaChars)]],
  });

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
    this.googlePlacesService.isCountrySelectedSig.set(true);
    this.googlePlacesService.countryAcrSig.set(member.countryAcr);
    this.googlePlacesService.countrySig.set(member.country);
    this.googlePlacesService.stateSig.set(member.state);
    this.googlePlacesService.citySig.set(member.city);
  }

  updateMemberAfterUpdateSucceed(): void {
    if (this.member) {
      this.member.knownAs = this.KnownAsCtrl.value;
      this.member.dateOfBirth = this.DateOfBirthCtrl.value;
      this.member.introduction = this.IntroductionCtrl.value;
      this.member.lookingFor = this.LookingForCtrl.value;
      this.member.interests = this.InterestsCtrl.value;
      this.member.countryAcr = this.googlePlacesService.countryAcrSig() as string;
      this.member.country = this.googlePlacesService.countrySig() as string;
      this.member.state = this.googlePlacesService.stateSig() as string;
      this.member.city = this.googlePlacesService.citySig() as string;
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
      && this.member.countryAcr === this.googlePlacesService.countryAcrSig()
      && this.member.country === this.googlePlacesService.countrySig()
      && this.member.state === this.googlePlacesService.stateSig()
      && this.member.city === this.googlePlacesService.citySig()
    ) {

      this.commonService.isPreventingLeavingPage = false

      return true;
    }

    this.commonService.isPreventingLeavingPage = true;

    return false;
  }

  updateUser(member: Member): void {
    if (member) {
      const updatedUser: UserUpdate = {
        username: member.userName,
        knownAs: this.KnownAsCtrl.value,
        dateOfBirth: this.DateOfBirthCtrl.value,
        introduction: this.IntroductionCtrl.value,
        lookingFor: this.LookingForCtrl.value,
        interests: this.InterestsCtrl.value,
        countryAcr: this.googlePlacesService.countryAcrSig() as string,
        country: this.googlePlacesService.countrySig() as string,
        state: this.googlePlacesService.stateSig() as string,
        city: this.googlePlacesService.citySig() as string,
      }

      this.userService.updateUser(updatedUser)
        .pipe(take(1))
        .subscribe({
          next: (response: ApiResponseMessage) => {
            if (response.message) {
              this.matSnak.open(response.message, "Close", { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000 });

              const loggedInUser: LoggedInUser | null = this.getLoggedInUserLocalStorage();

              if (loggedInUser) {
                loggedInUser.knownAs = this.KnownAsCtrl.value;

                this.accountService.setCurrentUser(loggedInUser);

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
