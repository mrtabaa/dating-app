import { Component, OnDestroy, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { Subscription } from 'rxjs';
import { MemberParams } from '../../../../models/helpers/member-params';
import { PaginatedResult } from '../../../../models/helpers/paginatedResult';
import { Pagination } from '../../../../models/helpers/pagination';
import { Member } from '../../../../models/member.model';
import { MemberService } from '../../../../services/member.service';
import { MatSliderModule } from '@angular/material/slider';
import { MatBottomSheet, MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatTabsModule } from '@angular/material/tabs';
import { OrderBottomSheetComponent } from './order-bottom-sheet/order-bottom-sheet.component';
import { FilterBottomSheetComponent } from './filter-bottom-sheet/filter-bottom-sheet.component';

@Component({
  selector: 'app-member-list-mobile',
  standalone: true,
  imports: [
    FormsModule, ReactiveFormsModule,
    OrderBottomSheetComponent, FilterBottomSheetComponent,
    MatPaginatorModule, MatSelectModule, MatButtonModule,
    MatIconModule, MatDividerModule, MatSliderModule,
    MatBottomSheetModule, MatTabsModule
  ],
  templateUrl: './member-list-mobile.component.html',
  styleUrl: './member-list-mobile.component.scss'
})
export class MemberListMobileComponent implements OnDestroy {
  private _memberService = inject(MemberService);
  private _fb = inject(FormBuilder);
  private _matBottomSheet = inject(MatBottomSheet);

  subscribed: Subscription | undefined;

  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;

  minAge: number = 18;
  maxAge: number = 99;

  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];

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

    this.initResetFilter();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }
  //#endregion auto-run methods

  //#region Reactive Form 
  orderByCtrl = this._fb.control('', [])

  filterFg = this._fb.group({
    genderCtrl: [],
    minAgeCtrl: [],
    maxAgeCtrl: []
  });

  get GenderCtrl(): AbstractControl {
    return this.filterFg.get('genderCtrl') as FormControl;
  }
  get MinAgeCtrl(): AbstractControl {
    return this.filterFg.get('minAgeCtrl') as FormControl;
  }
  get MaxAgeCtrl(): AbstractControl {
    return this.filterFg.get('maxAgeCtrl') as FormControl;
  }
  //#endregion Reactive form

  openOrderBottomSheet(): void {
    this._matBottomSheet.open(OrderBottomSheetComponent);
  }

  openFilterBottomSheet(): void {
    this._matBottomSheet.open(FilterBottomSheetComponent);
  }

  initResetFilter(): void {
    this.memberParams = this._memberService.getFreshMemberParams();

    if (this.memberParams?.orderBy)
      this.orderByCtrl.setValue(this.memberParams.orderBy);
    this.GenderCtrl.setValue(this.memberParams?.gender);
    this.MinAgeCtrl.setValue(this.memberParams?.minAge);
    this.MaxAgeCtrl.setValue(this.memberParams?.maxAge);

    this.getMembers();
  }

  getMembers(): void {
    this.updateMemberParams();

    this.subscribed = this._memberService.getMembers().subscribe({
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

      this._memberService.setMemberParams(this.memberParams);
      this.getMembers();
    }
  }

  updateMemberParams(): void {
    if (this.memberParams && this.orderByCtrl.value) {
      this.memberParams.orderBy = this.orderByCtrl.value;
      this.memberParams.gender = this.GenderCtrl.value;
      this.memberParams.minAge = this.MinAgeCtrl.value;
      this.memberParams.maxAge = this.MaxAgeCtrl.value;

      this._memberService.setMemberParams(this.memberParams);
    }
  }
}
