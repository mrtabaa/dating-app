import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../services/common.service';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MessagesUnreadComponent } from './messages-unread/messages-unread.component';
import { MessagesReadComponent } from './messages-read/messages-read.component';
import { MessagesSentComponent } from './messages-sent/messages-sent.component';
import { MessagesInboxComponent } from './messages-inbox/messages-inbox.component';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    CommonModule,
    MessagesInboxComponent, MessagesUnreadComponent, MessagesReadComponent, MessagesSentComponent,
    MatListModule, MatIconModule
  ],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit, OnDestroy {
  private _isMessageCompSig = inject(CommonService).isMessageCompSig;

  selectedTab = 'inbox';

  ngOnInit(): void {
    this._isMessageCompSig.set(true);
  }

  ngOnDestroy(): void {
    this._isMessageCompSig.set(false);
  }

  toggleTabs(tab: string): void {
    this.selectedTab = tab;
  }
}
