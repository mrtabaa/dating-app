import { Component, OnDestroy, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { Pagination } from 'src/app/models/helpers/pagination';
import { Member } from 'src/app/models/member.model';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit, OnDestroy {
  //#region Variables

  subscribed: Subscription | undefined;

  members: Member[] = [];
  pagination: Pagination | undefined;

  // Material Pagination attrs
  pageSize = 25;
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

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  loadMembers(): void {
    this.subscribed = this.memberService.getMembers(this.pageIndex + 1, this.pageSize).subscribe({
      next: response => {
        if (response.result && response.pagination) {
          this.members = response.result;
          this.pagination = response.pagination;
        }
      }
    });
  }

  handlePageEvent(e: PageEvent) {
    this.pageEvent = e;
    this.pageSize = e.pageSize;
    this.pageIndex = e.pageIndex;
    this.loadMembers();
  }
}
