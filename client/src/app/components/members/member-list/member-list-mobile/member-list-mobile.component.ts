import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {Subscription} from 'rxjs';
import {MemberParams} from '../../../../models/helpers/member-params';
import {PaginatedResult} from '../../../../models/helpers/paginatedResult';
import {Pagination} from '../../../../models/helpers/pagination';
import {Member} from '../../../../models/member.model';
import {MemberService} from '../../../../services/member.service';
import {MatBottomSheet, MatBottomSheetModule} from '@angular/material/bottom-sheet';
import {OrderBottomSheetComponent} from './order-bottom-sheet/order-bottom-sheet.component';
import {FilterBottomSheetComponent} from './filter-bottom-sheet/filter-bottom-sheet.component';
import {MemberCardMobileComponent} from '../../member-card/member-card-mobile/member-card-mobile.component';
import {AccountService} from "../../../../services/account.service";
import {MatSnackBar} from "@angular/material/snack-bar";

@Component({
  selector: 'app-member-list-mobile',
  imports: [
    MemberCardMobileComponent,
    MatPaginatorModule, MatButtonModule, MatIconModule, MatBottomSheetModule
  ],
  templateUrl: './member-list-mobile.component.html',
  styleUrl: './member-list-mobile.component.scss'
})
export class MemberListMobileComponent implements OnInit, OnDestroy {
  subsGetMembers: Subscription | undefined;
  subsOrderBottomSheet: Subscription | undefined;
  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;
  // Material Pagination attrs
  pageSizeOptions = [9, 15, 21];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = false;
  disabled = false;
  pageEvent: PageEvent | undefined;
  private _accountService = inject(AccountService);
  private _memberService = inject(MemberService);
  private _matBottomSheet = inject(MatBottomSheet);
  private _matSnackBar = inject(MatSnackBar);
  //#endregion

  //#region auto-run methods
  constructor() {
    this.initResetMemberParams();

    this.applyOrderFilter();
  }

  ngOnInit(): void {
    this.getMembers();
  }

  ngOnDestroy(): void {
    this.subsGetMembers?.unsubscribe();
    this.subsOrderBottomSheet?.unsubscribe();
  }

  //#endregion auto-run methods

  //#region BottomSheets
  openOrderBottomSheet(): void {
    this._matBottomSheet.open(OrderBottomSheetComponent);
  }

  openFilterBottomSheet(): void {
    this._matBottomSheet.open(FilterBottomSheetComponent);
  }

  applyOrderFilter(): void {
    this.subsOrderBottomSheet = this._memberService.eventEmitOrderFilterBottomSheet.pipe(
    ).subscribe(() => {
      this._matBottomSheet.dismiss();

      this.getMembers();
    });
  }

  //#endregion BottomSheets

  getMembers(): void {
    if (this.memberParams)
      this.subsGetMembers = this._memberService.getMembers(this.memberParams).subscribe({
        next: (response: PaginatedResult<Member[]>) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
  }

  initResetMemberParams(): void {
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

  handlePageEvent(e: PageEvent) {
    if (this.memberParams) {
      this.pageEvent = e;
      this.memberParams.pageSize = e.pageSize;
      this.memberParams.pageNumber = e.pageIndex + 1;

      this.getMembers();
    }
  }
}
