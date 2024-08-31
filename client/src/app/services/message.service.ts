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
  targetUserName: string | undefined;

  private readonly _joinGroup = "JoinGroup";
  private readonly _create = "Create";
  private readonly _sendMessage = "SendMessage";

  getInbox(messageParams: MessageParams): Observable<PaginatedResult<Message[]>> {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    if (messageParams.targetUserName)
      params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    return this._paginationHandler.getPaginatedResult<Message[]>(this._baseUrl, params);
  }

  /**
   * Set targetUserName to join two parties to a group.
   * Set viewport so scrollToBottom() works properly.
   * @param targetUserName
   * @param viewport 
   */
  setTargetUserNameAndViewPort(targetUserName: string, viewport: CdkVirtualScrollViewport): void {
    this.targetUserName = targetUserName;
    this.viewport = viewport;
  }

  createHubConnection(token: string): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      // .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('MessageHub connection started.');

        if (this.targetUserName && this.viewport)
          this.joinGroup();
      })
      .catch(err => console.error(err));

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

  // TODO If both parties are online, mark created messages as Read.
  async joinGroup(): Promise<void> {
    this.hubConnection?.invoke(this._joinGroup, this.targetUserName);
  }

  async create(messageIn: MessageIn): Promise<void> {
    this.newMessage = undefined; // reset each time a new message is sent. Set value at this.hubConnection.on(this._sendMessage
    this.hubConnection?.invoke(this._create, messageIn);
  }

  /**
   * This moves scroll to the bottom on the first messages load and any time a new message is sent.
   * Implemented in the service so it applies to the receiver of the message as well when this.hubConnection.on(this._sendMessage is triggered.
   */
  scrollToBottom() {
    try {
      setTimeout(() => {
        if (this.viewport)
          this.viewport.scrollToIndex(this.messagesSig().length - 1, 'smooth');
      }, 0);
    } catch (err) { console.error(err) }
  }
}

