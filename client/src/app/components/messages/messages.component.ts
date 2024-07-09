import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../services/common.service';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MessageService } from '../../services/message.service';
import { PaginationParams } from '../../models/helpers/paginationParams';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Message } from '../../models/message.model';
import { Pagination } from '../../models/helpers/pagination';
import { take } from 'rxjs/operators';
import { PaginatedResult } from '../../models/helpers/paginatedResult';
import { MatTableModule } from '@angular/material/table';
import { AccountService } from '../../services/account.service';
import { ShortenStringPipe } from '../../pipes/shorten-string.pipe';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    CommonModule,
    ShortenStringPipe,
    MatListModule, MatIconModule, MatPaginatorModule, MatTableModule
  ],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit, OnDestroy {
  private _loggedInUserSig = inject(AccountService).loggedInUserSig;
  private _isMessageCompSig = inject(CommonService).isMessageCompSig;
  private _messageService = inject(MessageService);

  selectedTab = 'inbox';
  params = new PaginationParams();
  displayedColumns: string[] = ['from', 'content', 'sentOn', 'readOn'];
  messages: Message[] = [];

  pagination: Pagination | undefined;
  pageSizeOptions = [5, 10, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  ngOnInit(): void {
    this._isMessageCompSig.set(true);
    this.getInbox();
  }

  ngOnDestroy(): void {
    this._isMessageCompSig.set(false);
  }

  toggleTabs(tab: string): void {
    this.selectedTab = tab;
  }

  getInbox(): void {
    this._messageService.getInbox(this.params)
      .pipe(
        take(1)
      ).subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            for (const message of response.result) {
              // if (message.senderUserName !== this._loggedInUserSig()?.userName) {
              this.messages.push(message);
              // }
            }

            console.log(this.messages);
            this.pagination = response.pagination;
          }
        }
      });
  }

  getUnread(): void {

  }

  getRead(): void {

  }

  getSent(): void {

  }
}
