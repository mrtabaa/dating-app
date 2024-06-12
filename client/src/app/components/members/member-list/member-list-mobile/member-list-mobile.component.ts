import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { MemberParams } from '../../../../models/helpers/member-params';
import { PaginatedResult } from '../../../../models/helpers/paginatedResult';
import { Pagination } from '../../../../models/helpers/pagination';
import { Member } from '../../../../models/member.model';
import { MemberService } from '../../../../services/member.service';
import { MatBottomSheet, MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { OrderBottomSheetComponent } from './order-bottom-sheet/order-bottom-sheet.component';
import { FilterBottomSheetComponent } from './filter-bottom-sheet/filter-bottom-sheet.component';
import { MemberCardMobileComponent } from '../../member-card/member-card-mobile/member-card-mobile.component';

@Component({
  selector: 'app-member-list-mobile',
  standalone: true,
  imports: [
    MemberCardMobileComponent, OrderBottomSheetComponent, FilterBottomSheetComponent,
    MatPaginatorModule, MatButtonModule, MatIconModule, MatBottomSheetModule
  ],
  templateUrl: './member-list-mobile.component.html',
  styleUrl: './member-list-mobile.component.scss'
})
export class MemberListMobileComponent implements OnInit, OnDestroy {
  private _memberService = inject(MemberService);
  private _matBottomSheet = inject(MatBottomSheet);

  subsGetMembers: Subscription | undefined;
  subsOrderBottomSheet: Subscription | undefined;

  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;

  private orderSheet = new OrderBottomSheetComponent();
  selectedOrder = this.orderSheet.orderByCtrl.value;

  // Material Pagination attrs
  pageSizeOptions = [9, 15, 21];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;
  //#endregion

  //#region auto-run methods
  constructor() {
    this.memberParams = this._memberService.getFreshMemberParams();

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
    this.subsGetMembers = this._memberService.getMembers().subscribe({
      next: (response: PaginatedResult<Member[]>) => {
        if (response.result && response.pagination) {
          this.members = response.result;
          this.pagination = response.pagination;

          console.log(this.members);
          console.log(this._memberService.memberParams);
        }
      }
    });
  }

  initResetFilter(): void {
    this._memberService.resetMemberParamsAndSignals();

    this.getMembers();
  }

  handlePageEvent(e: PageEvent) {
    if (this.memberParams) {
      this.pageEvent = e;
      this.memberParams.pageSize = e.pageSize;
      this.memberParams.pageNumber = e.pageIndex + 1;

      this._memberService.setMemberParams(this.memberParams);
      this.getMembers();
    }
  }
}
