import {AfterViewInit, Component, inject, Input, OnDestroy, OnInit, Signal, ViewChild} from '@angular/core';
import {Message} from '../../../models/message.model';
import {MessageService} from '../../../services/message.service';
import {take} from 'rxjs';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MessageParams} from '../../../models/helpers/message-params';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {MatIconModule} from '@angular/material/icon';
import {MessagePredicate} from '../../../enums/MessagePredicate.enum';
import {Member} from '../../../models/member.model';
import {IntlModule} from 'angular-ecmascript-intl';
import {MatDividerModule} from '@angular/material/divider';
import {MessageIn} from '../../../models/messageIn.model';
import {FormBuilder, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatButtonModule} from '@angular/material/button';
import {ResponsiveService} from '../../../services/responsive.service';
import {AccountService} from '../../../services/account.service';
import {CdkVirtualScrollViewport, ScrollingModule} from '@angular/cdk/scrolling';
import {CdkDynamicHeightDirective} from '../../../directives/cdk-dynamic-height.directive';
import {LoadingService} from '../../../services/loading.service';
import {v4 as uuidv4} from 'uuid';
import {CommonService} from '../../../services/common.service';
import {PaginatedResult} from '../../../models/helpers/paginatedResult';
import {MatSnackBar} from '@angular/material/snack-bar';
import {CdkTextareaAutosize} from "@angular/cdk/text-field";
import {MatInput} from "@angular/material/input";

@Component({
  selector: 'app-member-messages',
  imports: [
    CommonModule, NgOptimizedImage, ReactiveFormsModule, FormsModule,
    IntlModule, CdkDynamicHeightDirective,
    MatIconModule, MatPaginatorModule, MatDividerModule, MatFormFieldModule, MatButtonModule, ScrollingModule, CdkTextareaAutosize, MatInput
  ],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.scss'
})
export class MemberMessagesComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() memberIn: Member | undefined;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isLoadingSig = inject(LoadingService).isLoadingsig;
  isCreatingMessageSig = inject(CommonService).isCreatingMessageSig;
  bufferSize = 0;
  photo_WH = 40;
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;
  private _messageService = inject(MessageService);
  messagesSig: Signal<Message[]> = this._messageService.messagesSig;
  private _fb = inject(FormBuilder);
  createMessageCtrl = this._fb.control('',
    [Validators.required, Validators.maxLength(500)]); // nonEnter pattern
  private _snackBar = inject(MatSnackBar);
  private _totalPages = 1;
  private _defaultItemSize = 50;
  private readonly MAX_BUFFER_SIZE = 1000 * this._defaultItemSize; // Assuming 1000 messages as an upper limit
  private _isFirstLoad = true;
  private _messageParams = new MessageParams();

  ngOnInit(): void {
    this.createMessageHubConnection().finally();
    this.initMessageParams();
    this.initBufferSizeAndViewport();
    this.getMessages();
  }

  ngAfterViewInit(): void {
    this.setTargetUserNameAndViewport();
  }

  ngOnDestroy(): void {
    this._messageService.stopHubConnection().finally();
  }

  async createMessageHubConnection(): Promise<void> { // TODO: Rename all async methods to methodAsync
    const token = this.loggedInUserSig()?.token;
    if (token) {
      await this._messageService.createHubConnection(token);
    }
  }

  async create(): Promise<void> {
    this.isCreatingMessageSig.set(true); // disable loading ngx-spinner

    if (this.memberIn?.userName && this.createMessageCtrl.value) {
      const tempId = uuidv4(); // Generate a UUID

      const messageIn: MessageIn = {
        tempId: tempId,
        content: this.createMessageCtrl.value.trim(),
        receiverUserName: this.memberIn?.userName
      }

      //#region Create and add to messages for Optimistic approach
      const unsavedMessage: Message = {
        tempId: tempId, // to find the message from messages after API response to update the list's message props
        userOrTargetUserName: this.loggedInUserSig()?.userName,
        userOrTargetKnownAs: this.loggedInUserSig()?.knownAs,
        userOrTargetProfilePhoto: this.loggedInUserSig()?.profilePhotoUrl,
        content: messageIn.content,
        height: 0 // used in appCdkDynamicHeightDir to calculate a message height
      }
      //#endregion Create and add to messages for Optimistic approach

      // Send it to API
      try {
        // Send the message to the API
        await this._messageService.create(messageIn); // do NOT forget await to prevent race-conditions of dependant tasks

        if (this._messageService.newMessageRes) { // Keep unsavedMessage in messagesSig since it's added to DB
          this.isCreatingMessageSig.set(false); // enable loading ngx-spinner

          // temporarily increase the size to either of the length or the max size.
          this.scrollToEnd();
        } else { // delete message for API BadRequest response.
          this._messageService.messagesSig.update(messages => messages.filter(msg => msg.tempId !== unsavedMessage.tempId));
          this._snackBar.open('Sending failed. Try again, refresh the page or login again.', 'Close',
            {horizontalPosition: 'center', verticalPosition: 'top', duration: 10000});
        }

      } catch (error) {
        console.log(error);
      }

      this.createMessageCtrl.setValue(null); // Reset textarea and remove enter '\n' after being hit
    }
  }

  loadOlderMessages(event: number): void {
    if (this.viewport && !this._isFirstLoad) {
      const range = this.viewport.getRenderedRange();

      if (event === range.start) {
        this._messageParams.pageNumber++;
        this.getMessages();
        this.scrollToEnd();
      }
    }
  }

  initBufferSizeAndViewport(): void {
    // Set/Reset bufferSize for performance.
    this.bufferSize = this._messageParams.pageSize * this._defaultItemSize;
    this._messageService.viewport = this.viewport;
  }

  getMessages(): void {
    if (this._messageParams.pageNumber <= this._totalPages) {
      this._messageService.getInbox(this._messageParams)
        .pipe(
          take(1)
        ).subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            this._totalPages = response.pagination.totalPages;
            this._messageService.messagesSig.update(messages => {
              return [...(response.result?.reverse() ?? []), ...messages];
            });

            if (this._isFirstLoad && this.viewport) {
              this._messageService.scrollToBottom();
              this.initBufferSizeAndViewport();
              this._isFirstLoad = false;
            } else
              this.scrollToEnd();
          }
        }
      });
    }
  }

  private scrollToEnd() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex((this.messagesSig().length) - this._messageParams.pageSize * (this._messageParams.pageNumber - 1), 'smooth');
        }
      }, 0);
    } catch (err) {
      console.error(err)
    }
  }

  private initMessageParams(): void {
    this._messageService.messagesSig.set([]);
    this._messageParams.predicate = MessagePredicate.THREAD;
    this._messageParams.targetUserName = this.memberIn?.userName;
    this._messageParams.pageNumber = 1;
    this._messageParams.pageSize = 25;
  }

  private setTargetUserNameAndViewport(): void {
    if (this.memberIn && this.viewport)
      this._messageService.setTargetUserNameAndViewPort(this.memberIn.userName, this.viewport);
  }
}
