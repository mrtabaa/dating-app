import { AfterViewChecked, Component, effect, inject, ViewChild } from '@angular/core';
import { MemberListComponent } from '../members/member-list/member-list.component';
import { MemberListMobileComponent } from '../members/member-list/member-list-mobile/member-list-mobile.component';
import { FollowsComponent } from '../follows/follows.component';
import { MessagesComponent } from '../messages/messages.component';
import { AdminPanelComponent } from '../admin/admin-panel/admin-panel.component';
import { AccountService } from '../../services/account.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { ResponsiveService } from '../../services/responsive.service';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../services/common.service';
import { take } from 'rxjs';

@Component({
    selector: 'app-main',
    imports: [
        RouterModule, CommonModule,
        MemberListComponent, MemberListMobileComponent, FollowsComponent, MessagesComponent, AdminPanelComponent,
        MatTabsModule, MatIconModule
    ],
    templateUrl: './main.component.html',
    styleUrl: './main.component.scss'
})
export class MainComponent implements AfterViewChecked {
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup | undefined;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  private isNavMobileBrandClickedSig = inject(CommonService).isNavMobileBrandClickedSig;
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  initLoad = true;

  links = ['members', 'friends', 'messages', 'admin'];

  constructor() {
    // set tabGroup's index to 0 once nav-mobile brand is clicked
    effect(() => {
      if (this.tabGroup && this.isNavMobileBrandClickedSig()) {
        this.tabGroup.selectedIndex = 0;
        this.isNavMobileBrandClickedSig.set(false);
      }
    }, { allowSignalWrites: true });
  }

  ngAfterViewChecked(): void {
    if (this.initLoad && this.tabGroup) {
      this.setTabGroupParam(); // ViewChild is read in this lifeCycle only since it's in @if(async)

      this.initLoad = false;
    }
  }

  setTabGroupParam(): void {
    this.route.queryParams.pipe(
      take(1)).subscribe(params => {
        const tab = params['tab'];
        if (tab)
          this.setSelectTabIndex(tab);
      });
  }

  setSelectTabIndex(tabIndex: number): void {
    if (this.tabGroup) {
      this.tabGroup.selectedIndex = tabIndex;
      this.router.navigate([], { queryParams: { tab: tabIndex }, queryParamsHandling: 'merge' });
    }
  }
}