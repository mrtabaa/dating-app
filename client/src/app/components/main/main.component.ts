import { Component, inject } from '@angular/core';
import { MemberListComponent } from '../members/member-list/member-list.component';
import { MemberListMobileComponent } from '../members/member-list/member-list-mobile/member-list-mobile.component';
import { FollowsComponent } from '../follows/follows.component';
import { MessagesComponent } from '../messages/messages.component';
import { AdminPanelComponent } from '../admin/admin-panel/admin-panel.component';
import { AccountService } from '../../services/account.service';
import { RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { ResponsiveService } from '../../services/responsive.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [
    RouterModule,
    MemberListComponent, MemberListMobileComponent, FollowsComponent, MessagesComponent, AdminPanelComponent,
    MatTabsModule, MatIconModule
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;

  links = ['members', 'friends', 'messages', 'admin'];
}
