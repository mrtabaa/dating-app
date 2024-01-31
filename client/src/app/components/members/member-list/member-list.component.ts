import { Component, OnDestroy, OnInit, inject } from '@angular/core';
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
import { MatButtonModule } from '@angular/material/button';
import { range } from 'lodash';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    MemberCardComponent, FormsModule,
    MatPaginatorModule, MatSelectModule, MatButtonModule,
    MatIconModule, MatDividerModule
  ],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit, OnDestroy {
  //#region Variables
  private memberService = inject(MemberService);

  subscribed: Subscription | undefined;

  pagination: Pagination | undefined;
  members: Member[] | undefined;
  memberParams: MemberParams | undefined;

  // installed lodash library
  // TODO improve hard-coded numbers
  ages: number[] = [...range(18, 100)]; // Add 1 since lodash excludes last number. 100 => 99

  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];

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
    this.memberParams = this.memberService.getMemberParams();
  }

  ngOnInit(): void {
    this.getMembers();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }
  //#endregion auto-run methods

  resetFilter(): void {
    this.memberParams = this.memberService.resetMemberParams();

    this.getMembers();
  }

  getMembers(): void {
    this.setMemberParams();

    this.subscribed = this.memberService.getMembers().subscribe({
      next: response => {
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

  /**
   * Either send memberParams chosen by the user 
   * Or load the stored memberParams from localStorage for the page refreshes. 
   */
  private setMemberParams(): void {
    if (this.memberParams)
      this.memberService.setMemberParams(this.memberParams);

    else {
      const memberParams = localStorage.getItem('memberParams'); 
      
      if (memberParams)
      this.memberParams = JSON.parse(memberParams);
    }
  }
}
