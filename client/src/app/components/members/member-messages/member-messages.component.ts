import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from '../../../models/message.model';
import { MessageService } from '../../../services/message.service';
import { take } from 'rxjs';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MessageParams } from '../../../models/helpers/message-params';
import { Pagination } from '../../../models/helpers/pagination';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../pipes/shorten-string.pipe';
import { MessagePredicate } from '../../messages/MessageEnum.enum';
import { PaginatedResult } from '../../../models/helpers/paginatedResult';
import { Member } from '../../../models/member.model';
import { IntlModule } from 'angular-ecmascript-intl';
import { MatDividerModule } from '@angular/material/divider';
import { MessageIn } from '../../../models/messageIn.model';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { InputCvaComponent } from '../../_helpers/input-cva/input-cva.component';
import { ResponsiveService } from '../../../services/responsive.service';
import { AccountService } from '../../../services/account.service';
import { CreatedMessage } from '../../../models/createdMessage.model';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CdkDynamicHeightDirective } from '../../../directives/cdk-dynamic-height.directive';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, ReactiveFormsModule, FormsModule,
    ShortenStringPipe, IntlModule, InputCvaComponent, CdkDynamicHeightDirective,
    MatIconModule, MatPaginatorModule, MatDividerModule, MatFormFieldModule, MatButtonModule, ScrollingModule
  ],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.scss'
})
export class MemberMessagesComponent implements OnInit {
  @Input() memberIn: Member | undefined;
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;

  private _messageService = inject(MessageService);
  private fb = inject(FormBuilder);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  loggedInUserSig = inject(AccountService).loggedInUserSig;

  messages: Message[] = [];
  bufferSize = 0;
  defaultItemSize = 50;
  readonly MAX_BUFFER_SIZE = 1000 * this.defaultItemSize; // Assuming 1000 messages as an upper limit
  isFirstLoad = true;

  messageParams = new MessageParams();
  pagination: Pagination | undefined;

  photoWH = 40;

  createMessageCtrl = this.fb.control('', [Validators.maxLength(1000)]);

  ngOnInit(): void {
    this.initMessageParams();

    this.getMessages();
  }

  create(): void {
    if (this.memberIn?.userName && this.createMessageCtrl.value) {
      const messageIn: MessageIn = {
        content: this.createMessageCtrl.value,
        receiverUserName: this.memberIn?.userName
      }

      this._messageService.create(messageIn).pipe(
        take(1)
      ).subscribe({
        next: (createdMessage: CreatedMessage) => {
          if (createdMessage) {
            const message: Message = {
              Id: createdMessage.Id,
              userOrTargetUserName: this.loggedInUserSig()?.userName,
              userOrTargetKnownAs: this.loggedInUserSig()?.knownAs,
              userOrTargetProfilePhoto: this.loggedInUserSig()?.profilePhotoUrl,
              content: createdMessage.content,
              sentOn: createdMessage.sentOn,
              readOn: createdMessage.readOn
            }

            this.messages = [...this.messages, message];

            this.bufferSize = Math.min(this.messages.length * this.defaultItemSize, this.MAX_BUFFER_SIZE); // temprorarly increase the size to either of the length or the max size. 

            this.scrollToBottom();

            this.createMessageCtrl.setValue(null);
          }
        }
      });
    }
  }

  getMessages(): void {
    this._messageService.getInbox(this.messageParams)
      .subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            this.messages = [...response.result.reverse(), ...this.messages]; // reverse to sort messages from bottom(newer) to top(older)

            this.pagination = response.pagination;

            this.bufferSize = this.messageParams.pageSize * this.defaultItemSize; // 50 is the cdk's itemSize

            if (this.isFirstLoad) {

              this.scrollToBottom();

              this.isFirstLoad = false;
            }
            else {
              this.scrollToReloaded();
            }
          }
        }
      });
  }

  loadMoreMessages(event: number): void {
    if (event === 0 && !this.isFirstLoad && this.pagination?.totalItems && this.pagination.totalItems > this.messages.length) {
      this.messageParams.pageNumber++;
      this.getMessages();
    }
  }

  scrollToBottom() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex(this.messages.length - 1, 'smooth');
          this.bufferSize = this.messageParams.pageSize * this.defaultItemSize; // reset to the actual size for performance
        }
      }, 0);
    } catch (err) { console.error(err) }
  }

  scrollChangeIndex = 10;

  scrollToReloaded() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          if (this.messages.length > this.messageParams.pageSize) {

            this.viewport.scrollToIndex((this.messages.length - 1) - (this.messageParams.pageSize * (this.messageParams.pageNumber - 1)), 'instant');
          }
        }
      }, 0);
    } catch (err) { console.error(err) }
  }

  initMessageParams(): void {
    this.messageParams.predicate = MessagePredicate.THREAD;
    this.messageParams.targetUserName = this.memberIn?.userName;
    this.messageParams.pageNumber = 1;
    this.messageParams.pageSize = 25;
  }
}
