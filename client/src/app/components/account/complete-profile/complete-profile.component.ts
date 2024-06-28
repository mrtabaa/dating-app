import { Component, OnInit, Signal, ViewChild, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';
import { MatExpansionModule } from '@angular/material/expansion';
import { Router, RouterLink } from '@angular/router';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';
import { PhotoEditorComponent } from '../../user/photo-editor/photo-editor.component';
import { MemberService } from '../../../services/member.service';
import { take } from 'rxjs';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { Member } from '../../../models/member.model';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { UserUpdate } from '../../../models/user-update.model';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { UserService } from '../../../services/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AccountService } from '../../../services/account.service';
import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { GooglePlacesService } from '../../../services/google-places.service';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { GooglePlacesComponent } from '../../google-places/google-places.component';
import { ResponsiveService } from '../../../services/responsive.service';
import { PhotoEditorMobileComponent } from '../../user/photo-editor/photo-editor-mobile/photo-editor-mobile.component';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [
    FormsModule, ReactiveFormsModule, CommonModule, NgOptimizedImage,
    PhotoEditorComponent, PhotoEditorMobileComponent, GooglePlacesComponent,
    MatStepperModule, InputCvaComponent, RouterLink, MatButtonModule, MatInputModule,
    MatExpansionModule, MatIconModule, MatCardModule, MatDividerModule
  ],
  templateUrl: './complete-profile.component.html',
  styleUrl: './complete-profile.component.scss',
  providers: [{
    provide: STEPPER_GLOBAL_OPTIONS, useValue: { displayDefaultIndicatorType: false }
  }]
})
export class CompleteProfileComponent implements OnInit {
  @ViewChild(GooglePlacesComponent) private googlePlacesComponent: GooglePlacesComponent | undefined;

  private fb = inject(FormBuilder);
  private memberService = inject(MemberService);
  private userService = inject(UserService);
  private accountService = inject(AccountService);
  private googlePlacesService = inject(GooglePlacesService);
  private router = inject(Router);
  private matSnack = inject(MatSnackBar);
  isMobileSig = inject(ResponsiveService).isMobileSig;

  readonly maxTextAreaChars: number = 1000;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 50;
  readonly starterLable = 'Starter';
  readonly starterSummary = 'Your basic information';
  readonly photosLabel = 'Photos';
  readonly introductionLabel = 'Introduction';
  readonly interestsLabel = 'Interests';
  readonly lookingForLabel = 'Looking for';
  readonly emptyWarning = 'No info is entered. Consider updating to attract more matches.';

  member: Member | undefined;
  panelOpenState = false;
  isUploading = false;

  countrySig: Signal<string | undefined> = this.googlePlacesService.countrySig;
  countryAcrSig: Signal<string | undefined> = this.googlePlacesService.countryAcrSig;
  stateSig: Signal<string | undefined> = this.googlePlacesService.stateSig;
  citySig: Signal<string | undefined> = this.googlePlacesService.citySig;
  isCountrySelectedSig: Signal<boolean> = this.googlePlacesService.isCountrySelectedSig;

  ngOnInit(): void {
    this.getMember();
  }

  starterFg = this.fb.group({
    knownAsCtrl: [null, [Validators.required, Validators.maxLength(this.maxInputChars)]]
  })
  introductionCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  interestsCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  lookingForCtrl = this.fb.control('', [Validators.maxLength(this.maxTextAreaChars)]);
  photosCtrl = this.fb.control(null);

  get KnownAsCtrl(): AbstractControl {
    return this.starterFg.get('knownAsCtrl') as FormControl;
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

  getMember(): void {
    const loggedInUserStr = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

      this.memberService.getMemberByUsername(loggedInUser.userName)?.pipe(
        take(1))
        .subscribe(member => {
          if (member) {
            this.member = member;

            this.initControllers(member);
          }
        });
    }
  }

  initControllers(member: Member): void {
    this.KnownAsCtrl.setValue(member.knownAs);
    this.IntroductionCtrl.setValue(member.introduction);
    this.InterestsCtrl.setValue(member.interests);
    this.LookingForCtrl.setValue(member.lookingFor);
  }

  saveAngNext(): void {
    const updatedUser: UserUpdate = this.createUpdatedUser();
    updatedUser.isProfileCompleted = false;

    this.userService.updateUser(updatedUser)
      .pipe(take(1)).subscribe();
  }

  submit(): void {
    if (this.countrySig() && this.stateSig() && this.citySig()) {
      const updatedUser: UserUpdate = this.createUpdatedUser();
      updatedUser.isProfileCompleted = true;

      this.userService.updateUser(updatedUser)
        .pipe(take(1))
        .subscribe({
          next: (response: ApiResponseMessage) => {
            if (response.message) {
              const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');
              if (loggedInUserStr) {
                const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

                loggedInUser.isProfileCompleted = true;

                this.router.navigate(['/main']);

                this.accountService.setCurrentUser(loggedInUser);
              }

              this.matSnack.open(response.message, 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000 });
            }
          }
        });
    }
  }

  private createUpdatedUser(): UserUpdate {
    return {
      knownAs: this.KnownAsCtrl.value,
      countryAcr: this.countryAcrSig() as string,
      country: this.countrySig() as string, // Type assertion since countrySig() is never null here
      state: this.stateSig() as string,
      city: this.citySig() as string,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingForCtrl.value,
      interests: this.InterestsCtrl.value,
      isProfileCompleted: false
    }
  }

  resetCountry(): void {
    this.googlePlacesService.resetCountry();
    this.googlePlacesComponent?.searchedLocationCtrl.reset();

    this.countrySig = this.googlePlacesService.countrySig;
    this.isCountrySelectedSig = this.googlePlacesService.isCountrySelectedSig;
  }

  /**
   * Receives isUploading event from PhotoEditor to disable the Next button while a photo isUploading
   * @param $isUploading
   */
  setIsUploadingPhoto($isUploading: boolean): void {
    this.isUploading = $isUploading;
  }
}
