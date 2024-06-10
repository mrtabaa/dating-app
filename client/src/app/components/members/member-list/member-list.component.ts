import { Component, OnDestroy, inject } from '@angular/core';
import { PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { Pagination } from '../../../models/helpers/pagination';
import { PaginatedResult } from "../../../models/helpers/paginatedResult";
import { Member } from '../../../models/member.model';
import { MemberCardComponent } from '../member-card/member-card.component';
import { MemberService } from '../../../services/member.service';
import { MemberParams } from '../../../models/helpers/member-params';
import { MatSelectModule } from '@angular/material/select';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { FollowModifiedEmit } from '../../../models/helpers/follow-modified-emit';
import { FollowService } from '../../../services/follow.service';
import { MatSliderModule } from '@angular/material/slider';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [
    MemberCardComponent, FormsModule, ReactiveFormsModule,
    MatPaginatorModule, MatSelectModule, MatButtonModule,
    MatIconModule, MatDividerModule, MatSliderModule
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnDestroy {
  //#region Variables
  private memberService = inject(MemberService);
  private followService = inject(FollowService);
  private fb = inject(FormBuilder);

  subscribed: Subscription | undefined;

  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;

  minAge: number = 18;
  maxAge: number = 99;

  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];

  // Material Pagination attrs
  pageSizeOptions = [5, 10, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;
  //#endregion

  //#region auto-run methods
  constructor() {
    this.memberParams = this.memberService.getFreshMemberParams();

    this.initResetFilter();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }
  //#endregion auto-run methods

  //#region Reactive Form 
  filterFg = this.fb.group({
    orderByCtrl: [],
    genderCtrl: [],
    minAgeCtrl: [],
    maxAgeCtrl: []
  });

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
  //#endregion Reactive form

  initResetFilter(): void {
    this.memberParams = this.memberService.getFreshMemberParams();

    this.OrderByCtrl.setValue(this.memberParams?.orderBy);
    this.GenderCtrl.setValue(this.memberParams?.gender);
    this.MinAgeCtrl.setValue(this.memberParams?.minAge);
    this.MaxAgeCtrl.setValue(this.memberParams?.maxAge);

    this.getMembers();
  }

  getMembers(): void {
    this.updateMemberParams();

    this.subscribed = this.memberService.getMembers().subscribe({
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

      this.memberService.setMemberParams(this.memberParams);
      this.getMembers();
    }
  }

  updateMemberParams(): void {
    if (this.memberParams) {
      this.memberParams.orderBy = this.OrderByCtrl.value;
      this.memberParams.gender = this.GenderCtrl.value;
      this.memberParams.minAge = this.MinAgeCtrl.value;
      this.memberParams.maxAge = this.MaxAgeCtrl.value;

      this.memberService.setMemberParams(this.memberParams);
    }
  }

  modifyFollowUnfollowIcon($event: FollowModifiedEmit): void {
    if (this.members)
      this.members = this.followService.modifyFollowUnfollowIcon(this.members, $event);
  }
}
