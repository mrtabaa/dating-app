import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MessageService } from '../../services/message.service';
import { PaginationParams } from '../../models/helpers/paginationParams';
import { CommonService } from '../../services/common.service';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { MessagesUnreadComponent } from './messages-unread/messages-unread.component';
import { MessagesReadComponent } from './messages-read/messages-read.component';
import { MessagesSentComponent } from './messages-sent/messages-sent.component';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, RouterModule,
    MessagesUnreadComponent, MessagesReadComponent, MessagesSentComponent,
    MatListModule, MatIconModule
  ],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit, OnDestroy {
  private _messageService = inject(MessageService);
  private _isMessageCompSig = inject(CommonService).isMessageCompSig;

  selectedTab = 'unread';
  pageParams = new PaginationParams();

  ngOnInit(): void {
    this._isMessageCompSig.set(true);

    this.pageParams.pageSize = 2;
    this.getInboxMessages();
  }

  ngOnDestroy(): void {
    this._isMessageCompSig.set(false);
  }

  getInboxMessages(): void {
    this._messageService.getInboxMessages(this.pageParams);
  }

  toggleTabs(tab: string): void {
    this.selectedTab = tab;
  }
}
