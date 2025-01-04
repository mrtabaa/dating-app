import {Component, inject, OnDestroy} from '@angular/core';
import {MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {Subscription} from 'rxjs';
import {Pagination} from '../../../models/helpers/pagination';
import {PaginatedResult} from "../../../models/helpers/paginatedResult";
import {Member} from '../../../models/member.model';
import {MemberCardComponent} from '../member-card/member-card.component';
import {MemberService} from '../../../services/member.service';
import {MemberParams} from '../../../models/helpers/member-params';
import {MatSelectModule} from '@angular/material/select';
import {AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatDividerModule} from '@angular/material/divider';
import {FollowModifiedEmit} from '../../../models/helpers/follow-modified-emit';
import {FollowService} from '../../../services/follow.service';
import {MatSliderModule} from '@angular/material/slider';
import {InputCvaComponent} from "../../_helpers/input-cva/input-cva.component";
import {AccountService} from "../../../services/account.service";
import {MatSnackBar} from "@angular/material/snack-bar";

@Component({
  selector: 'app-member-list',
  imports: [
    MemberCardComponent, FormsModule, ReactiveFormsModule,
    MatPaginatorModule, MatSelectModule, MatButtonModule,
    MatIconModule, MatDividerModule, MatSliderModule, InputCvaComponent
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnDestroy {
  subscribed: Subscription | undefined;
  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;
  minAge: number = 18;
  maxAge: number = 99;
  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];
  // Material Pagination attrs
  pageSizeOptions = [5, 9, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;
  //#region Variables
  private _accountService = inject(AccountService);
  private _memberService = inject(MemberService);
  private _followService = inject(FollowService);
  private _fb = inject(FormBuilder);
  //#region Reactive Form
  filterFg = this._fb.group({
    userNameOrKnownAs: ['', []],
    orderByCtrl: [],
    genderCtrl: [],
    minAgeCtrl: [],
    maxAgeCtrl: []
  });
  private _matSnackBar = inject(MatSnackBar);
  //#endregion

  //#endregion auto-run methods

  //#region auto-run methods
  constructor() {
    this.initResetMemberParamsAndFilterControllers();
    this.getMembers();
  }

  get UserNameOrKnownAs(): FormControl {
    return this.filterFg.get('userNameOrKnownAs') as FormControl;
  }

  get OrderByCtrl(): AbstractControl {
    return this.filterFg.get('orderByCtrl') as FormControl;
  }

  get GenderCtrl(): AbstractControl {
    return this.filterFg.get('genderCtrl') as FormControl;
  }

  get MinAgeCtrl(): AbstractControl {
    return this.filterFg.get('minAgeCtrl') as FormControl;
  }

  get MaxAgeCtrl(): AbstractControl {
    return this.filterFg.get('maxAgeCtrl') as FormControl;
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  //#endregion Reactive form

  initResetMemberParamsAndFilterControllers(): void {
    this.initResetMemberParams();

    if (this.memberParams) {
      this.UserNameOrKnownAs.setValue(undefined);
      this.OrderByCtrl.setValue(this.memberParams.orderBy);
      this.GenderCtrl.setValue(undefined); // To keep filter by userNameOrKnownAs gender-independent
      this.MinAgeCtrl.setValue(this.memberParams.minAge);
      this.MaxAgeCtrl.setValue(this.memberParams.maxAge);
    }
  }

  getMembers(): void {
    if (this.memberParams)
      this.subscribed = this._memberService.getMembers(this.memberParams).subscribe({
        next: (response: PaginatedResult<Member[]>) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
  }

  handlePageEvent(e: PageEvent) {
    if (this.memberParams) {
      this.pageEvent = e;
      this.memberParams.pageSize = e.pageSize;
      this.memberParams.pageNumber = e.pageIndex + 1;

      this.getMembers();
    }
  }

  updateMemberParams(): void {
    if (this.memberParams) {
      this.memberParams.userNameOrKnownAs = this.UserNameOrKnownAs.value;
      this.memberParams.orderBy = this.OrderByCtrl.value;
      this.memberParams.gender = this.GenderCtrl.value;
      this.memberParams.minAge = this.MinAgeCtrl.value;
      this.memberParams.maxAge = this.MaxAgeCtrl.value;
    }
  }

  modifyFollowUnfollowIcon($event: FollowModifiedEmit): void {
    if (this.members)
      this.members = this._followService.modifyFollowUnfollowIcon(this.members, $event);
  }

  isAnyFilterApplied(): boolean {
    console.log('PARAMS', this.memberParams);
    console.log('FILTER', this.filterFg.value);

    return this.OrderByCtrl.value === this.memberParams?.orderBy &&
      (this.UserNameOrKnownAs.pristine || this.UserNameOrKnownAs.value?.length < 1) &&
      (this.GenderCtrl.pristine || this.GenderCtrl.value === this.memberParams?.gender) &&
      this.MinAgeCtrl.value === this.memberParams?.minAge &&
      this.MaxAgeCtrl.value === this.memberParams?.maxAge
  }

  private initResetMemberParams(): void {
    // retrieve gender from localStorage since accountService.loggedInUserSig is ran after this service and gender will be undefined.
    const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const gender: string = JSON.parse(loggedInUserStr).gender;

      this.memberParams = new MemberParams(gender ? gender : 'male'); // 'male' for the admin who doesn't have a gender
    } else {
      this._matSnackBar.open('Your login has expired. Please login again!', 'Close', {
        horizontalPosition: 'center',
        verticalPosition: 'top',
        duration: 7000
      });
      this._accountService.logout();
    }
  }
}
