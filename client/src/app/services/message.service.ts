import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Message } from '../models/message.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { MessageParams } from '../models/helpers/message-params';
import { MessageIn } from '../models/messageIn.model';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _hubUrl: string = environment.hubUrl + 'message';
  hubConnection: HubConnection | undefined;
  private _paginationHandler = new PaginationHandler();
  newMessage: Message | undefined;
  messages: Message[] = [];
  messagesSig = signal<Message[]>([]);
  viewport: CdkVirtualScrollViewport | undefined;

  private readonly _messageThread = "MessageThread";
  private readonly _sendMessage = "SendMessage";

  async create(messageIn: MessageIn): Promise<void> {
    this.newMessage = undefined; // reset each time a new message is sent. Set value at this.hubConnection.on(this._sendMessage
    this.hubConnection?.invoke('Create', messageIn);
  }

  getInbox(messageParams: MessageParams): Observable<PaginatedResult<Message[]>> {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    if (messageParams.targetUserName)
      params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    return this._paginationHandler.getPaginatedResult<Message[]>(this._baseUrl, params);
  }

  createHubConnection(token: string): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('Hub connection started.'))
      .catch(err => console.log(err));

    this.hubConnection.on(this._sendMessage, (message: Message) => {
      if (message) {
        this.newMessage = message; // Use to update optimistic approach in MemberMessagesComponent. Delete the message if api failed.
        this.messagesSig.update(msgs => [...msgs, message]);
        this.scrollToBottom();
      }
    });
  }

  stopHubConnection(): void {
    this.hubConnection?.stop();
  }

  scrollToBottom() {
    try {
      setTimeout(() => {
        if (this.viewport) {
          this.viewport.scrollToIndex(this.messagesSig().length - 1, 'smooth');
        }
      }, 0);
    } catch (err) { console.error(err) }
  }
}

