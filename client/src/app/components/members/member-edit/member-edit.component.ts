import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription, take } from 'rxjs';
import { UpdateResult } from 'src/app/models/helpers/update-result.model';
import { MemberUpdate } from 'src/app/models/user-update.model';
import { user } from 'src/app/models/user.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/user.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.scss']
})
export class MemberEditComponent implements OnInit, OnDestroy {
  user$: Observable<user> | undefined;
  apiPhotoUrl = environment.apiPhotoUrl;
  user: User | null = null;
  subscribed: Subscription | undefined;

  readonly minTextAreaChars: number = 10;
  readonly maxTextAreaChars: number = 500;
  readonly minInputChars: number = 3;
  readonly maxInputChars: number = 30;

  constructor(
    accountService: AccountService,
    private memberService: MemberService,
    private fb: FormBuilder,
    private matSnak: MatSnackBar) {
    accountService.currentUser$.pipe(
      take(1)).subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    if (this.user) {
      this.user$ = this.memberService.getMember(this.user.email);
    }

    this.initContollersValues();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  memberEditFg: FormGroup = this.fb.group({
    introductionCtrl: ['', [Validators.required, Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    lookingForCtrl: ['', [Validators.required, Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    interestsCtrl: ['', [Validators.minLength(this.minTextAreaChars), Validators.maxLength(this.maxTextAreaChars)]],
    cityCtrl: ['', [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]],
    countryCtrl: ['', [Validators.required, Validators.minLength(this.minInputChars), Validators.maxLength(this.maxInputChars)]]
  });

  get IntroductionCtrl(): AbstractControl {
    return this.memberEditFg.get('introductionCtrl') as FormControl;
  }
  get LookingForCtrl(): AbstractControl {
    return this.memberEditFg.get('lookingForCtrl') as FormControl;
  }
  get InterestsCtrl(): AbstractControl {
    return this.memberEditFg.get('interestsCtrl') as FormControl;
  }
  get CityCtrl(): AbstractControl {
    return this.memberEditFg.get('cityCtrl') as FormControl;
  }
  get CountryCtrl(): AbstractControl {
    return this.memberEditFg.get('countryCtrl') as FormControl;
  }

  initContollersValues() {
    if (this.user$ && this.IntroductionCtrl.pristine) {
      this.subscribed = this.user$.subscribe((user: user) => {
        this.IntroductionCtrl.setValue(user.introduction);
        this.LookingForCtrl.setValue(user.lookingFor);
        this.InterestsCtrl.setValue(user.interests);
        this.CityCtrl.setValue(user.city);
        this.CountryCtrl.setValue(user.country);
      });
    }
  }

  updateMember(): void {

    let updatedMember: MemberUpdate = {
      email: this.user?.email,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingForCtrl.value,
      interests: this.InterestsCtrl.value,
      city: this.CityCtrl.value,
      country: this.CountryCtrl.value
    }

    this.subscribed = this.memberService.updateMember(updatedMember).subscribe({
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

    this.memberEditFg.markAsPristine();
  }

  logForm() {
    console.log(this.IntroductionCtrl)
  }
}
