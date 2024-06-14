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
import { FollowPredicate } from '../../models/helpers/follow-predicate';
import { FollowModifiedEmit } from '../../models/helpers/follow-modified-emit';
import { FollowParams } from '../../models/helpers/follow-params';
import { ResponsiveService } from '../../services/responsive.service';
import { FollowsMobileComponent } from './follows-mobile/follows-mobile.component';

@Component({
  selector: 'app-follows',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MemberCardComponent, FollowsMobileComponent,
    MatTabsModule, MatPaginatorModule
  ],
  templateUrl: './follows.component.html',
  styleUrl: './follows.component.scss'
})
export class FollowsComponent implements OnInit {
  private followService = inject(FollowService);
  loading = inject(LoadingService);
  isMobileSig = inject(ResponsiveService).isMobileSig;

  subscribed: Subscription | undefined;
  members: Member[] | undefined;

  followPredicate = FollowPredicate;
  followParams: FollowParams | undefined;
  
  pagination: Pagination | undefined;
  pageSizeOptions = [5, 9, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  ngOnInit(): void {
    this.followParams = new FollowParams();

    this.getFollows();
  }

  getFollows(): void {
    this.members = []; // reset on each tab select

    if (this.followParams)
      this.subscribed = this.followService.getFollows(this.followParams).subscribe({
        next: (response: PaginatedResult<Member[]>) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      }
      );
  }

  modifyFollowUnfollowIcon($event: FollowModifiedEmit): void {
    if (this.members)
      this.members = this.followService.modifyFollowUnfollowIcon(this.members, $event);
  }

  /**
   * Set predicate based on each tab then call getFollows()
   * @param event 
   */
  onTabChange(event: MatTabChangeEvent) { // called on tab change
    if (event.tab.textLabel === "Following") {
      if (this.followParams)
        this.followParams.predicate = FollowPredicate.followings;
    }
    else {
      if (this.followParams)
        this.followParams.predicate = FollowPredicate.followers;
    }

    this.getFollows()
  }

  handlePageEvent(e: PageEvent) {
    if (this.followParams) {
      if (e.pageSize !== this.followParams.pageSize)
        e.pageIndex = 0;

      this.pageEvent = e;
      this.followParams.pageSize = e.pageSize;
      this.followParams.pageNumber = e.pageIndex + 1;

      this.getFollows();
    }
  }
}
