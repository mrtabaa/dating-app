import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { Subscription, take } from 'rxjs';
import { FollowModifiedEmit } from '../../../models/helpers/follow-modified-emit';
import { FollowParams } from '../../../models/helpers/follow-params';
import { FollowPredicate } from '../../../models/helpers/follow-predicate';
import { PaginatedResult } from '../../../models/helpers/paginatedResult';
import { Pagination } from '../../../models/helpers/pagination';
import { Member } from '../../../models/member.model';
import { FollowService } from '../../../services/follow.service';
import { LoadingService } from '../../../services/loading.service';
import { ResponsiveService } from '../../../services/responsive.service';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberCardComponent } from '../../members/member-card/member-card.component';
import { ShortenStringPipe } from '../../../pipes/shorten-string.pipe';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MemberService } from '../../../services/member.service';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-follows-mobile',
  standalone: true,
  imports: [
    CommonModule, FormsModule, NgOptimizedImage,
    MemberCardComponent, FollowsMobileComponent,
    ShortenStringPipe,
    MatTabsModule, MatPaginatorModule, MatIconModule, MatButtonModule
  ],
  templateUrl: './follows-mobile.component.html',
  styleUrl: './follows-mobile.component.scss'
})
export class FollowsMobileComponent implements OnInit {
  @Output() FollowModifiedOut = new EventEmitter<FollowModifiedEmit>();

  private _followService = inject(FollowService);
  private _memberService = inject(MemberService);
  loading = inject(LoadingService);
  snackBar = inject(MatSnackBar);
  isMobileSig = inject(ResponsiveService).isMobileSig;

  subscribed: Subscription | undefined;
  members: Member[] | undefined;

  followPredicate = FollowPredicate;
  followParams: FollowParams | undefined;
  imageWH = 50;

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
      this.subscribed = this._followService.getFollows(this.followParams).subscribe({
        next: (response: PaginatedResult<Member[]>) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      }
      );
  }

  // modifyFollowUnfollowIcon($event: boolean): void {
  //   if (this.members)
  //     this.members = this._followService.modifyFollowUnfollowIcon(this.members, $event);
  // }

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

  addFollow(member: Member): void {
    this._followService.addFollow(member.userName)
      .pipe(
        take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          this.snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          });

          member.isFollowing = true;
        }
      });
  }

  removeFollow(member: Member): void {
    this._followService.removeFollow(member.userName)
      .pipe(
        take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          this.snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          });

          member.isFollowing = false;
        }
      });
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
