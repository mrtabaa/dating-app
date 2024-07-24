import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from '../../../models/message.model';
import { MessageService } from '../../../services/message.service';
import { take } from 'rxjs';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MessageParams } from '../../../models/helpers/message-params';
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
import { LoadingService } from '../../../services/loading.service';
import { v4 as uuidv4 } from 'uuid';
import { CommonService } from '../../../services/common.service';

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
  isLoadingSig = inject(LoadingService).isLoadingsig;
  isCreatingMessageSig = inject(CommonService).isCreatingMessageSig;

  messages: Message[] = [];
  bufferSize = 0;
  defaultItemSize = 50;
  readonly MAX_BUFFER_SIZE = 1000 * this.defaultItemSize; // Assuming 1000 messages as an upper limit
  isFirstLoad = true;

  messageParams = new MessageParams();

  photoWH = 40;

  createMessageCtrl = this.fb.control('', [Validators.maxLength(1000)]);

  ngOnInit(): void {
    this.initMessageParams();
    this.initBufferSize();

    this.getMessages();
  }

  create(): void {
    this.isCreatingMessageSig.set(true); // disable loading ngx-spinner

    if (this.memberIn?.userName && this.createMessageCtrl.value) {
      const tempId = uuidv4(); // Generate a UUID

      const messageIn: MessageIn = {
        tempId: tempId,
        content: this.createMessageCtrl.value,
        receiverUserName: this.memberIn?.userName
      }

      //#region Create and add to messages for Optimistic approach
      const message: Message = {
        tempId: tempId, // to find the message from messages after API response to update the list's message props
        userOrTargetUserName: this.loggedInUserSig()?.userName,
        userOrTargetKnownAs: this.loggedInUserSig()?.knownAs,
        userOrTargetProfilePhoto: this.loggedInUserSig()?.profilePhotoUrl,
        content: messageIn.content,
        sentOn: new Date()
      }

      this.messages = [...this.messages, message];

      // temprorarly increase the size to either of the length or the max size. 
      this.bufferSize = Math.min(this.messages.length * this.defaultItemSize, this.MAX_BUFFER_SIZE);

      this.scrollToBottom();

      this.createMessageCtrl.setValue(null);
      //#endregion Create and add to messages for Optimistic approach

      // Send it to API
      this._messageService.create(messageIn).pipe(
        take(1)
      ).subscribe({
        next: (createdMessage: CreatedMessage) => {
          if (createdMessage) {
            // Update message of the messages with API validated values
            const index = this.messages.findIndex((msg: Message) => msg.tempId === message.tempId);

            this.messages[index].Id = createdMessage.Id;
            this.messages[index].sentOn = createdMessage.sentOn;
            this.messages[index].readOn = createdMessage.readOn;

            delete this.messages[index].tempId; // Remove the tempId once updated 

            setTimeout(() => {
              this.isCreatingMessageSig.set(false); // enable loading ngx-spinner
            }, 100);
          }
        },
        // delete message for API BadRequest response. 
        error: () => this.messages = this.messages.filter(msg => msg.tempId !== message.tempId)
      });
    }
  }

  getMessages(): void {
    this._messageService.getInbox(this.messageParams).pipe(
      take(1)
    ).subscribe({
      next: (response: PaginatedResult<Message[]>) => {
        if (response.result && response.pagination) {
          this.messages = [...response.result.reverse(), ...this.messages]; // reverse to sort messages from bottom(newer) to top(older)

          if (this.isFirstLoad)
            this.scrollToBottom();
          else
            this.scrollToReloaded();
        }
      }
    });
  }

  loadOlderMessages(event: number): void {
    if (this.viewport && !this.isFirstLoad) {
      const range = this.viewport.getRenderedRange();

      if (event === range.start) {
        this.messageParams.pageNumber++;
        this.getMessages();
        this.scrollToReloaded();
      }
    }
  }

  scrollToBottom() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex(this.messages.length - 1, 'smooth');

          this.initBufferSize();
          this.isFirstLoad = false;
        }
      }, 0);
    } catch (err) { console.error(err) }
  }

  scrollToReloaded() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex((this.messages.length) - this.messageParams.pageSize * (this.messageParams.pageNumber - 1), 'instant');
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

  initBufferSize(): void {
    // Set/Reset bufferSize for performance.
    this.bufferSize = this.messageParams.pageSize * this.defaultItemSize;
  }
}