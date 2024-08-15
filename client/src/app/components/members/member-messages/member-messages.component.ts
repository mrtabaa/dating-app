import { AfterViewInit, Component, inject, Input, OnDestroy, OnInit, Signal, ViewChild } from '@angular/core';
import { Message } from '../../../models/message.model';
import { MessageService } from '../../../services/message.service';
import { Subscription, take } from 'rxjs';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MessageParams } from '../../../models/helpers/message-params';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../pipes/shorten-string.pipe';
import { MessagePredicate } from '../../../enums/MessagePredicate.enum';
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
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CdkDynamicHeightDirective } from '../../../directives/cdk-dynamic-height.directive';
import { LoadingService } from '../../../services/loading.service';
import { v4 as uuidv4 } from 'uuid';
import { CommonService } from '../../../services/common.service';
import { PaginatedResult } from '../../../models/helpers/paginatedResult';
import { MatSnackBar } from '@angular/material/snack-bar';

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
export class MemberMessagesComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() memberIn: Member | undefined;
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;

  private _messageService = inject(MessageService);
  private _fb = inject(FormBuilder);
  private _snackBar = inject(MatSnackBar);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;
  isCreatingMessageSig = inject(CommonService).isCreatingMessageSig;

  messagesSig: Signal<Message[]> = this._messageService.messagesSig;
  private _messagesSubs: Subscription | undefined;
  private _totalPages = 1;

  bufferSize = 0;
  private _defaultItemSize = 50;
  private readonly MAX_BUFFER_SIZE = 1000 * this._defaultItemSize; // Assuming 1000 messages as an upper limit
  private _isFirstLoad = true;

  private _messageParams = new MessageParams();

  photo_WH = 40;

  createMessageCtrl = this._fb.control('', [Validators.maxLength(1000)]);

  ngOnInit(): void {
    this.initMessageParams();
    this.initBufferSizeAndViewport();
    this.getMessages();
  }

  ngAfterViewInit(): void {
    this.setTargetUserNameAndViewport();
    this.createHubConnection();
  }

  ngOnDestroy(): void {
    this._messagesSubs?.unsubscribe();
    this._messageService.stopHubConnection();
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
      }

      this.createMessageCtrl.setValue(null);
      //#endregion Create and add to messages for Optimistic approach

      // Send it to API
      this._messageService.create(messageIn);

      setTimeout(() => {
        const newMessage = this._messageService.newMessage;

        if (newMessage) {
          // Update message of the messages with API validated values

          this.isCreatingMessageSig.set(false); // enable loading ngx-spinner

          // temprorarly increase the size to either of the length or the max size. 
          this.bufferSize = Math.min(this.messagesSig().length * this._defaultItemSize, this.MAX_BUFFER_SIZE);
        }
        else {
          // delete message for API BadRequest response. 
          this._messageService.messagesSig.update(messages => messages.filter(msg => msg.tempId !== message.tempId));
          this._snackBar.open('Sending message failed. Check your internet connection of login agian.', 'Close',
            { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
        }
      }, 500);
    }
  }

  private getMessages(): void {
    if (this._messageParams.pageNumber <= this._totalPages) {
      this._messageService.getInbox(this._messageParams)
        .pipe(
          take(1)
        ).subscribe({
          next: (response: PaginatedResult<Message[]>) => {
            if (response.result && response.pagination) {
              this._totalPages = response.pagination.totalPages;
              this._messageService.messagesSig.update(messages => [...(response.result?.reverse() ?? []), ...messages]);

              if (this._isFirstLoad && this.viewport) {
                this._messageService.scrollToBottom();
                this.initBufferSizeAndViewport();
                this._isFirstLoad = false;
              }
              else
                this.scrollToReloaded();
            }
          }
        });
    }
  }

  loadOlderMessages(event: number): void {
    if (this.viewport && !this._isFirstLoad) {
      const range = this.viewport.getRenderedRange();

      if (event === range.start) {
        this._messageParams.pageNumber++;
        this.getMessages();
        this.scrollToReloaded();
      }
    }
  }

  private scrollToReloaded() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex((this.messagesSig().length) - this._messageParams.pageSize * (this._messageParams.pageNumber - 1), 'instant');
        }
      }, 0);
    } catch (err) { console.error(err) }
  }

  private initMessageParams(): void {
    this._messageParams.predicate = MessagePredicate.THREAD;
    this._messageParams.targetUserName = this.memberIn?.userName;
    this._messageParams.pageNumber = 1;
    this._messageParams.pageSize = 25;
  }

  initBufferSizeAndViewport(): void {
    // Set/Reset bufferSize for performance.
    this.bufferSize = this._messageParams.pageSize * this._defaultItemSize;
    this._messageService.viewport = this.viewport;
  }

  private createHubConnection(): void {
    const token = this.loggedInUserSig()?.token;
    if (token)
      this._messageService.createHubConnection(token);
  }

  private setTargetUserNameAndViewport(): void {
    if (this.memberIn && this.viewport)
      this._messageService.setTargetUserNameAndViewPort(this.memberIn.userName, this.viewport);
  }
}