import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { take } from 'rxjs';
import { MessageParams } from '../../../models/helpers/message-params';
import { Message } from '../../../models/message.model';
import { AccountService } from '../../../services/account.service';
import { LoadingService } from '../../../services/loading.service';
import { MessageService } from '../../../services/message.service';
import { MessagePredicate } from '../../../enums/MessagePredicate.enum';
import { Member } from '../../../models/member.model';
import { CdkVirtualScrollerComponent } from '../helpers/cdk-virtual-scroller/cdk-virtual-scroller.component';
import { PaginatedResult } from '../../../models/helpers/paginatedResult';

@Component({
  selector: 'app-messages-mobile',
  standalone: true,
  imports: [
    CdkVirtualScrollerComponent,
    MatTabsModule, MatIconModule
  ],
  templateUrl: './messages-mobile.component.html',
  styleUrl: './messages-mobile.component.scss'
})
export class MessagesMobileComponent implements OnInit {
  @Input() memberIn: Member | undefined;
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup | undefined;
  private _messageService = inject(MessageService);
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;
  messages: Message[] = [];
  totalItemsCount = 0;

  messageParams = new MessageParams();

  ngOnInit(): void {
    this.initMessageParams();

    this.getMessages();
  }

  setSelectTabIndex(tabIndex: number): void {
    this.messageParams.predicate = tabIndex;
    this.messageParams.pageNumber = 1; // reset on tab change
    this.messages = []; // reset on tab change
    this.getMessages();
  }

  getMessages(): void {
    this._messageService.getInbox(this.messageParams)
      .pipe(
        take(1)
      ).subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            this.messages = [...this.messages, ...response.result];
            this.totalItemsCount = response.pagination.totalItems;
          }
        }
      });
  }

  initMessageParams(): void {
    this.messageParams.predicate = MessagePredicate.INBOX;
    this.messageParams.targetUserName = this.memberIn?.userName;
    this.messageParams.pageNumber = 1;
    this.messageParams.pageSize = 25;
  }
}
