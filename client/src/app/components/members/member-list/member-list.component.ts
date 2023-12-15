import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { Pagination } from '../../../models/helpers/pagination';
import { Member } from '../../../models/member.model';
import { MemberCardComponent } from '../member-card/member-card.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MemberService } from '../../../services/member.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    MemberCardComponent,
    MatPaginatorModule
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit, OnDestroy {
  //#region Variables
  private memberService = inject(MemberService);

  subscribed: Subscription | undefined;

  members: Member[] = [];
  pagination: Pagination | undefined;

  // Material Pagination attrs
  pageSize = 5;
  pageIndex = 0; // add 1 before sending to API since endpoint's pageNumber starts from 1
  pageSizeOptions = [5, 10, 25];

  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;

  pageEvent: PageEvent | undefined;

  //#endregion

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  loadUsers(): void {
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
    this.loadUsers();
  }
}
