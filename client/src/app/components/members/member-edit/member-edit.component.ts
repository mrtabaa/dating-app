import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, Subscription, take } from 'rxjs';
import { UpdateResult } from 'src/app/models/helpers/update-result.model';
import { MemberUpdate } from 'src/app/models/member-update.model';
import { Member } from 'src/app/models/member.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.scss']
})
export class MemberEditComponent implements OnInit, OnDestroy {
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.memberEditFg.dirty) {
      $event.returnValue = true;
    }
  }

  member$: Observable<Member> | undefined;
  user: User | null = null;
  mainUrl: string | undefined;
  form: FormGroup | undefined;
  subscribed: Subscription | undefined;

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
      this.member$ = this.memberService.getMember(this.user.email);
    }

    this.initContollersValues();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  memberEditFg: FormGroup = this.fb.group({
    introductionCtrl: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    lookingCtrl: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    interestsCtrl: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    cityCtrl: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
    countryCtrl: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]]
  });

  get IntroductionCtrl(): AbstractControl {
    return this.memberEditFg.get('introductionCtrl') as FormControl;
  }
  get LookingCtrl(): AbstractControl {
    return this.memberEditFg.get('lookingCtrl') as FormControl;
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
    if (this.member$ && this.IntroductionCtrl.pristine) {
      this.subscribed = this.member$.subscribe((member: Member) => {
        this.IntroductionCtrl.setValue(member.introduction);
        this.LookingCtrl.setValue(member.lookingFor);
        this.InterestsCtrl.setValue(member.interests);
        this.CityCtrl.setValue(member.city);
        this.CountryCtrl.setValue(member.country);
      });
    }
  }

  updateMember(): void {

    let updatedMember: MemberUpdate = {
      email: this.user?.email,
      introduction: this.IntroductionCtrl.value,
      lookingFor: this.LookingCtrl.value,
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
}
