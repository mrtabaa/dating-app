import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Observable, Subscription, take } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UpdateResult } from '../../../models/helpers/update-result.model';
import { UserUpdate } from '../../../models/user-update.model';
import { Member } from '../../../models/member.model';
import { AccountService } from '../../../services/account.service';
import { PhotoEditorComponent } from '../../user/photo-editor/photo-editor.component';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { User } from '../../../models/user.model';
import { MemberService } from '../../../services/member.service';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, FormsModule, ReactiveFormsModule,
    PhotoEditorComponent,
    MatCardModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatButtonModule
  ],
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.scss']
})
export class UserEditComponent implements OnInit, OnDestroy {
  private accountService = inject(AccountService);
  private userService = inject(UserService);
  private memberService = inject(MemberService);
  private fb = inject(FormBuilder);
  private matSnak = inject(MatSnackBar);

  member$: Observable<Member> | undefined;
  apiPhotoUrl = environment.apiPhotoUrl;
  user: User | null = null;
  subscribed: Subscription | undefined;

  readonly minTextAreaChars: number = 10;
  readonly maxTextAreaChars: number = 500;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 30;

  constructor() {
    this.accountService.currentUser$.pipe(
      take(1)).subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    if (this.user) {
      this.member$ = this.memberService.getMember(this.user.email);
    }

    this.initContollersValues();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  userEditFg: FormGroup = this.fb.group({
    introductionCtrl: ['', [Validators.required, Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    lookingForCtrl: ['', [Validators.required, Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    interestsCtrl: ['', [Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    cityCtrl: ['', [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]],
    countryCtrl: ['', [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]]
  });

  get IntroductionCtrl(): AbstractControl {
    return this.userEditFg.get('introductionCtrl') as FormControl;
  }
  get LookingForCtrl(): AbstractControl {
    return this.userEditFg.get('lookingForCtrl') as FormControl;
  }
  get InterestsCtrl(): AbstractControl {
    return this.userEditFg.get('interestsCtrl') as FormControl;
  }
  get CityCtrl(): AbstractControl {
    return this.userEditFg.get('cityCtrl') as FormControl;
  }
  get CountryCtrl(): AbstractControl {
    return this.userEditFg.get('countryCtrl') as FormControl;
  }

  initContollersValues() {
    if (this.member$ && this.IntroductionCtrl.pristine) {
      this.subscribed = this.member$.subscribe((user: Member) => {
        this.IntroductionCtrl.setValue(user.introduction);
        this.LookingForCtrl.setValue(user.lookingFor);
        this.InterestsCtrl.setValue(user.interests);
        this.CityCtrl.setValue(user.city);
        this.CountryCtrl.setValue(user.country);
      });
    }
  }

  updateUser(): void {

    let updatedUser: UserUpdate = {
      email: this.user?.email,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingForCtrl.value,
      interests: this.InterestsCtrl.value,
      city: this.CityCtrl.value,
      country: this.CountryCtrl.value
    }

    this.subscribed = this.userService.updateUser(updatedUser).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.isAcknowledged && updateResult.modifiedCount > 0) {
          this.matSnak.open("Successfully updated.", "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000
          });
        }
        else {
          this.matSnak.open("Update failed.", "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 10000
          });
        }
      },
      error: (errors: any[]) => {
        for (const errorCategory of errors) {
          for (const error of errorCategory) {
            console.log(error);
          }
        }
      }
    });

    this.userEditFg.markAsPristine();
  }

  logForm() {
    console.log(this.IntroductionCtrl)
  }
}
