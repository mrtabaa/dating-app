import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute } from '@angular/router';
import { take } from 'rxjs';
import { MessageParams } from '../../../models/helpers/message-params';
import { Message } from '../../../models/message.model';
import { AccountService } from '../../../services/account.service';
import { LoadingService } from '../../../services/loading.service';
import { MessageService } from '../../../services/message.service';
import { MessagePredicate } from '../MessageEnum.enum';
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
  messages: Message[] = [];
  private _messageService = inject(MessageService);
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;

  private route = inject(ActivatedRoute);
  initLoad = true;

  messageParams = new MessageParams();

  photoWH = 40;

  ngOnInit(): void {
    this.initMessageParams();

    this.getMessages();
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

      this.messageParams.predicate = tabIndex;
    }
  }

  getMessages(): void {
    this._messageService.getInbox(this.messageParams).pipe(
      take(1)
    ).subscribe({
      next: (response: PaginatedResult<Message[]>) => {
        if (response.result && response.pagination) {
          this.messages = [...this.messages, ...response.result];
        }
      }
    });
  }

  initMessageParams(): void {
    this.messageParams.predicate = MessagePredicate.INBOX;
    this.messageParams.targetUserName = this.memberIn?.userName;
    this.messageParams.pageNumber = 1;
    this.messageParams.pageSize = 9;
  }
}
