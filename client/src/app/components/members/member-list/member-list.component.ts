import { Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Observable, map } from 'rxjs';
import { Pagination } from 'src/app/models/helpers/pagination';
import { Member } from 'src/app/models/member.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit {
  //#region Variables

  members$: Observable<Member[] | null> | undefined;
  members: Member[] = [];
  pagination: Pagination | undefined;

  // Material Pagination attrs
  length = 50;
  pageSize = 5;
  pageIndex = 0; // add 1 before sending to API since endpoint's pageNumber starts from 1
  pageSizeOptions = [5, 10, 25];

  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;

  pageEvent: PageEvent | undefined;

  //#endregion

  constructor(private memberService: MemberService) { }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(): void {
    this.members$ = this.memberService.getMembers(this.pageIndex + 1, this.pageSize).pipe(
      map(response => {
        if (response.result && response.pagination) {
          this.pagination = response.pagination;
          return (response.result);
        }

        return null;
      }));
  }

  handlePageEvent(e: PageEvent) {
    this.pageEvent = e;
    this.length = e.length;
    this.pageSize = e.pageSize;
    this.pageIndex = e.pageIndex;
    this.loadMembers();
  }
}
