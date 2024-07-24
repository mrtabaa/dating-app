import { Component, effect, inject, ViewChild } from '@angular/core';
import { MemberListComponent } from '../members/member-list/member-list.component';
import { MemberListMobileComponent } from '../members/member-list/member-list-mobile/member-list-mobile.component';
import { FollowsComponent } from '../follows/follows.component';
import { MessagesComponent } from '../messages/messages.component';
import { AdminPanelComponent } from '../admin/admin-panel/admin-panel.component';
import { AccountService } from '../../services/account.service';
import { RouterModule } from '@angular/router';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { ResponsiveService } from '../../services/responsive.service';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../services/common.service';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [
    RouterModule, CommonModule,
    MemberListComponent, MemberListMobileComponent, FollowsComponent, MessagesComponent, AdminPanelComponent,
    MatTabsModule, MatIconModule
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup | undefined;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  private isNavMobileBrandClickedSig = inject(CommonService).isNavMobileBrandClickedSig;

  links = ['members', 'friends', 'messages', 'admin'];

  constructor() {
    effect(() => {
      if (this.tabGroup && this.isNavMobileBrandClickedSig()) {
        this.tabGroup.selectedIndex = 0;
        this.isNavMobileBrandClickedSig.set(false);
      }
    }, { allowSignalWrites: true });
  }
}