import { Component, OnDestroy, effect, inject } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { Pagination } from '../../../models/helpers/pagination';
import { Member } from '../../../models/member.model';
import { MemberCardComponent } from '../member-card/member-card.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MemberService } from '../../../services/member.service';
import { MemberParams } from '../../../models/helpers/member-params';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../services/account.service';
import { MatButtonModule } from '@angular/material/button';
import { range } from 'lodash';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    MemberCardComponent, FormsModule,
    MatPaginatorModule, MatSelectModule, MatButtonModule, MatIconModule
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnDestroy {
  //#region Variables
  private memberService = inject(MemberService);
  private accountService = inject(AccountService);

  subscribed: Subscription | undefined;

  pagination: Pagination | undefined;
  loggedInGender: string | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;
  gender: string | undefined;
  minAge: number = 18;
  maxAge: number = 99;

  // installed lodash library
  ages: number[] = [...range(this.minAge, this.maxAge + 1)]; // Add 1 since lodash excludes last number. 100 => 99

  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];
  orderBy: string = this.orderOptions[0];

  // Material Pagination attrs
  // pageSize = 5;
  // pageIndex = 0; // add 1 before sending to API since endpoint's pageNumber starts from 1
  pageSizeOptions = [5, 10, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  //#endregion

  //#region auto-run methods
  constructor() {
    effect(() => {
      this.loggedInGender = this.accountService.loggedInUserSig()?.gender;

      if (this.loggedInGender) {
        this.memberParams = new MemberParams(this.loggedInGender);
        this.gender = this.memberParams.gender;
      }

      this.getMembers();
    });
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }
  //#endregion auto-run methods

  getFilteredMembers(): void {
    if (!this.gender)
      this.gender = this.memberParams?.gender;

    if (this.memberParams && this.gender) {
      this.memberParams = {
        pageNumber: this.memberParams.pageNumber,
        pageSize: this.memberParams.pageSize,
        gender: this.gender,
        minAge: this.minAge,
        maxAge: this.maxAge,
        orderBy: this.orderBy
      }
    }

    this.getMembers();
  }

  resetFilter(): void {
    if (this.loggedInGender)
      this.memberParams = new MemberParams(this.loggedInGender);

    if (this.memberParams) {
      this.gender = this.memberParams.gender;
      this.minAge = this.memberParams.minAge;
      this.maxAge = this.memberParams.maxAge;
      this.orderBy = this.memberParams.orderBy;
    }

    this.getMembers();
  }

  disableFilterBotton(): boolean {
    if (this.loggedInGender) {
      const memberParams = new MemberParams(this.loggedInGender);
      if (this.minAge > this.maxAge ||
        (
          this.gender == memberParams.gender
          && this.minAge === memberParams.minAge
          && this.maxAge === memberParams.maxAge
        )
      ) {
        return true
      }
    }

    return false;
  }

  disableResetBotton(): boolean {
    if (this.loggedInGender) {
      const memberParams = new MemberParams(this.loggedInGender);
      if (
        this.gender == memberParams.gender
        && this.minAge === memberParams.minAge
        && this.maxAge === memberParams.maxAge
      ) {
        return true
      }
    }

    return false;
  }

  getMembers(): void {
    if (this.memberParams) {
      this.subscribed = this.memberService.getMembers(this.memberParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
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
