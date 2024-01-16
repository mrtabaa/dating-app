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

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    MemberCardComponent, FormsModule,
    MatPaginatorModule, MatSelectModule
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnDestroy {
  //#region Variables
  private memberService = inject(MemberService);
  private accountService = inject(AccountService);

  subscribed: Subscription | undefined;

  members: Member[] = [];

  pagination: Pagination | undefined;
  memberParams: MemberParams | undefined;
  gender: string | undefined;
  maxAge: number | undefined;
  minAge: number | undefined;

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
      const loggedInGender = this.accountService.loggedInUserSig()?.gender;
      if (loggedInGender)
        this.memberParams = new MemberParams(loggedInGender);

      this.loadMembers();
    });
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }
  //#endregion auto-run methods

  loadFilteredMembers(): void {
    if (this.memberParams && this.gender) {
      console.log(this.gender);
      this.memberParams = {
        pageNumber: this.memberParams.pageNumber,
        pageSize: this.memberParams.pageSize,
        gender: this.gender,
        minAge: this.memberParams.minAge,
        maxAge: this.memberParams.maxAge
      }
    }

    this.loadMembers();
  }

  loadMembers(): void {
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

      this.loadMembers();
    }
  }
}
