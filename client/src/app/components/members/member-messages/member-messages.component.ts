import { AfterViewChecked, Component, ElementRef, inject, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from '../../../models/message.model';
import { MessageService } from '../../../services/message.service';
import { take } from 'rxjs';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
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

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, ReactiveFormsModule, FormsModule,
    ShortenStringPipe, IntlModule, InputCvaComponent,
    MatIconModule, MatPaginatorModule, MatDividerModule, MatFormFieldModule, MatButtonModule
  ],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.scss'
})
export class MemberMessagesComponent implements OnInit, AfterViewChecked {
  @Input() memberIn: Member | undefined;
  @ViewChild('messagesContainer') private messagesContainer: ElementRef | undefined;

  private _messageService = inject(MessageService);
  private fb = inject(FormBuilder);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  loggedInUserSig = inject(AccountService).loggedInUserSig;

  messages: Message[] = [];

  messageParams = new MessageParams();
  pagination: Pagination | undefined;
  pageSizeOptions = [9, 25, 50];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;

  photoWH = 40;

  createMessageCtrl = this.fb.control('', [Validators.required, Validators.maxLength(500)]);

  ngOnInit(): void {
    this.initMessageParams();

    this.getMessages();
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
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
              readOn: createdMessage.readOn,
            }

            this.messages.push(message);
          }
        }
      });
    }
  }

  getMessages(): void {
    this.messages = []; // reset

    this._messageService.getInbox(this.messageParams)
      .pipe(
        take(1)
      ).subscribe({
        next: (response: PaginatedResult<Message[]>) => {
          if (response.result && response.pagination) {
            // this.messages = response.result.reverse(); // reverse to sort messages from bottom(newer) to top(older)
            this.messages = [...this.messages, ...response.result.reverse()]; // reverse to sort messages from bottom(newer) to top(older)
            this.pagination = response.pagination;
          }
        }
      });
  }

  loadMoreMessages(): void {
    // this.messagesContainer?.nativeElement
  }

  scrollToBottom() {
    try {
      if (this.messagesContainer)
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
    } catch (err) { console.error(err) }
  }

  initMessageParams(): void {
    this.messageParams.predicate = MessagePredicate.THREAD;
    this.messageParams.targetUserName = this.memberIn?.userName;
    this.messageParams.pageNumber = 1;
    this.messageParams.pageSize = 25;
  }

  handlePagination(): void {
    this.messageParams.pageNumber++;
    this.messageParams.pageSize += 25;

    this.getMessages();
  }
}
