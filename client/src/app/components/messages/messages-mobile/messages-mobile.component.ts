// import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { AfterViewChecked, Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { Router, ActivatedRoute } from '@angular/router';
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
  private _messageService = inject(MessageService);
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  initLoad = true;

  messages: Message[] = [];
  bufferSize = 0;
  // defaultItemSize = 50;
  // readonly MAX_BUFFER_SIZE = 1000 * this.defaultItemSize; // Assuming 1000 messages as an upper limit
  // isFirstLoad = true;

  messageParams = new MessageParams();

  photoWH = 40;

  ngOnInit(): void {
    this.initMessageParams();
      // this.initBufferSize();

      this.getMessages();
  }

  // ngAfterViewChecked(): void {
  //   if (this.initLoad && this.tabGroup) {
  //     this.setTabGroupParam(); // ViewChild is read in this lifeCycle only since it's in @if(async)

  //     this.initLoad = false;
  //   }
  // }

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
      // this.router.navigate([], { queryParams: { tab: tabIndex }, queryParamsHandling: 'merge' });

      this.messageParams.predicate = tabIndex;
    }
  }

  getMessages(): void {
    this._messageService.getInbox(this.messageParams).pipe(
      take(1)
    ).subscribe({
      next: (response: PaginatedResult<Message[]>) => {
        if (response.result && response.pagination) {
          this.messages = response.result;

          // this.scrollToReloaded();
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

  // initBufferSize(): void {
  //   // Set/Reset bufferSize for performance.
  //   this.bufferSize = this.messageParams.pageSize * this.defaultItemSize;
  // }
}
