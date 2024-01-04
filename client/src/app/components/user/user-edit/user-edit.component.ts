import { Component, OnDestroy, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Subscription, take } from 'rxjs';
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
export class UserEditComponent implements OnDestroy {
  private accountService = inject(AccountService);
  private userService = inject(UserService);
  private memberService = inject(MemberService);
  private fb = inject(FormBuilder);
  private matSnak = inject(MatSnackBar);

  apiPhotoUrl = environment.apiPhotoUrl;
  subscribedMember: Subscription | undefined;
  member: Member | undefined;

  readonly minTextAreaChars: number = 10;
  readonly maxTextAreaChars: number = 500;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 30;

  ngOnInit(): void {
    this.getMember();
  }

  ngOnDestroy(): void {
    this.subscribedMember?.unsubscribe();
  }

  getMember(): void {
    this.accountService.getLoggedInUser().pipe(take(1)).subscribe(loggedInUser => {
      if (loggedInUser) {
        this.memberService.getMember(loggedInUser.id).pipe(take(1)).subscribe(member => {
          if (member) {
            this.member = member;
            this.initContollersValues(member);
          }
        });
      }
    });
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

  initContollersValues(member: Member) {
    this.IntroductionCtrl.setValue(member.introduction);
    this.LookingForCtrl.setValue(member.lookingFor);
    this.InterestsCtrl.setValue(member.interests);
    this.CityCtrl.setValue(member.city);
    this.CountryCtrl.setValue(member.country);
  }

  updateUser(member: Member): void {
    if (member) {
      let updatedUser: UserUpdate = {
        email: member.email,
        introduction: this.IntroductionCtrl.value,
        lookingFor: this.LookingForCtrl.value,
        interests: this.InterestsCtrl.value,
        city: this.CityCtrl.value,
        country: this.CountryCtrl.value
      }

      this.subscribedMember = this.userService.updateUser(updatedUser).subscribe({
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
  }

  logForm() {
    console.log(this.IntroductionCtrl)
  }
}
