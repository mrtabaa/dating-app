import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { CommonService } from '../../services/common.service';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MessageService } from '../../services/message.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Message } from '../../models/message.model';
import { Pagination } from '../../models/helpers/pagination';
import { take } from 'rxjs/operators';
import { PaginatedResult } from '../../models/helpers/paginatedResult';
import { MatTableModule } from '@angular/material/table';
import { ShortenStringPipe } from '../../pipes/shorten-string.pipe';
import { Tabs } from './tabs.enum';
import { MessageParams } from '../../models/helpers/message-params';
import { LoadingService } from '../../services/loading.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, RouterModule,
    ShortenStringPipe,
    MatListModule, MatIconModule, MatPaginatorModule, MatTableModule
  ],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit, OnDestroy {
  private _isMessageCompSig = inject(CommonService).isMessageCompSig;
  private _messageService = inject(MessageService);
  isLoadingSig = inject(LoadingService).isLoadingsig;

  Tabs = Tabs;
  selectedTab = Tabs.inbox;

  displayedColumns: string[] = ['from', 'content', 'sentOn', 'readOn'];
  messages: Message[] = [];

  messageParams = new MessageParams();
  pagination: Pagination | undefined;
  pageSizeOptions = [9, 25, 50];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  ngOnInit(): void {
    this._isMessageCompSig.set(true);
    this.getMessages();
  }

  ngOnDestroy(): void {
    this._isMessageCompSig.set(false);
  }

  toggleTabs(tab: number): void {
    this.selectedTab = tab;
    this.messageParams.predicate = tab;

    this.getMessages();
  }

  getMessages(): void {
    this.messages = []; // reset

    this._messageService.getInbox(this.messageParams)
      .pipe(
        take(1)
      ).subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            this.messages = response.result;
            this.pagination = response.pagination;
          }
        }
      });
  }

  handlePageEvent(e: PageEvent) {
    if (this.messageParams) {
      this.pageEvent = e;
      this.messageParams.pageSize = e.pageSize;
      this.messageParams.pageNumber = e.pageIndex + 1;

      this.getMessages();
    }
  }
}
