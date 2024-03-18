import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTabsModule, MatTabChangeEvent } from '@angular/material/tabs';
import { Subscription } from 'rxjs';
import { Member } from '../../models/member.model';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { FollowService } from '../../services/follow.service';
import { Pagination } from '../../models/helpers/pagination';
import { PaginatedResult } from "../../models/helpers/paginatedResult";
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { LoadingService } from '../../services/loading.service';

@Component({
  selector: 'app-follows',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MemberCardComponent,
    MatTabsModule, MatPaginatorModule
  ],
  templateUrl: './follows.component.html',
  styleUrl: './follows.component.scss'
})
export class FollowsComponent implements OnInit {
  private followService = inject(FollowService);
  loading = inject(LoadingService);

  predicate: string = 'followings';
  isMembersValid: boolean = true;
  subscribed: Subscription | undefined;
  members: Member[] | undefined;

  pagination: Pagination | undefined;
  pageNumber = 1;
  pageSize = 5;
  pageSizeOptions = [5, 10, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  ngOnInit(): void {
    this.getFollows();
  }

  getFollows(): void {
    this.isMembersValid = true; // reset to default.
    this.members = []; // reset on each tab select

    this.subscribed = this.followService.getFollows(this.predicate).subscribe({
      next: (response: PaginatedResult<Member[]>) => {
        if (response.result && response.pagination) {
          this.members = response.result;
          this.pagination = response.pagination;
          this.isMembersValid = this.members ? true : false;
        }
      }
    }
    );
  }

  /**
   * Set predicate based on each tab then call getFollows()
   * @param event 
   */
  onTabChange(event: MatTabChangeEvent) { // called on tab change
    if (event.tab.textLabel == "Following") {
      this.predicate = 'followings';
      this.getFollows();
    }
    else {
      this.predicate = 'followers';
      this.getFollows()
    }
  }

  handlePageEvent(e: PageEvent) {
    this.pageEvent = e;
    this.pageSize = e.pageSize;
    this.pageNumber = e.pageIndex + 1;

    this.getFollows();
  }
}
