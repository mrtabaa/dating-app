import {inject, Injectable, signal} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Message} from '../models/message.model';
import {PaginatedResult} from '../models/helpers/paginatedResult';
import {PaginationHandler} from '../extensions/paginationHandler';
import {MessageParams} from '../models/helpers/message-params';
import {MessageIn} from '../models/messageIn.model';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {CdkVirtualScrollViewport} from '@angular/cdk/scrolling';
import {AccountService} from "./account.service";

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  hubConnection: HubConnection | undefined;
  newMessageRes: Message | undefined;
  messagesSig = signal<Message[]>([]);
  viewport: CdkVirtualScrollViewport | undefined;
  targetUserName: string | undefined;
  private _loggedInUserSig = inject(AccountService).loggedInUserSig;
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _hubUrl: string = environment.hubUrl + 'message';
  private _paginationHandler = new PaginationHandler();
  private readonly _joinGroup = "JoinGroup";
  private readonly _UpdatedReadOn = "UpdatedReadOn";
  private readonly _create = "Create";
  private readonly _newMessageRes = "NewMessageRes";

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

  async createHubConnection(token: string): Promise<void> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start()
      .then(() => console.log('MessageHub connection started.'));

    await this.joinGroup()
      .then(() => console.log(this._loggedInUserSig()?.userName, 'joined the chat.'))
      .catch(err => console.log(this._loggedInUserSig()?.userName, 'failed to joined the chat with error:', err));

    this.getUpdatedReadOn();
    this.getNewMessageRes();
  }

  getNewMessageResFromHub(): void {
    this.hubConnection?.on(this._newMessageRes, (messageRes: Message) => {
      if (messageRes) {
        console.log(messageRes);
        this.newMessageRes = messageRes; // Use to update optimistic approach in MemberMessagesComponent. Delete the message if api failed.
        this.messagesSig.update(messages => [...messages, messageRes]);

        this.scrollToBottom();
      }
    });
  }

  // TODO If both parties are online, mark created messages as Read.
  // TODO Implement delete message.
  async joinGroup(): Promise<void> {
    await this.hubConnection?.invoke(this._joinGroup, this.targetUserName);
  }

  getUpdatedReadOn(): void {
    this.hubConnection?.off(this._UpdatedReadOn); // Remove existing listener to prevent memory leak
    this.hubConnection?.on(this._UpdatedReadOn, (updatedReadOn: Date): void => {
      const readOnDate = new Date(updatedReadOn); // convert SignalR ISO string to Date object

      this.messagesSig.update(messages => // implicit return
        messages.map(message => ({
          ...message,
          readOn: readOnDate
        }))
      )
    });
  }

  async create(messageIn: MessageIn): Promise<void> {
    this.newMessageRes = undefined; // reset each time a new message is sent. Set value at this.hubConnection.on(this._sendMessage
    await this.hubConnection?.invoke(this._create, messageIn);
  }

  getNewMessageRes(): void {
    this.hubConnection?.off(this._newMessageRes); // Remove existing listener to prevent memory leak
    this.hubConnection?.on(this._newMessageRes, (messageRes: Message) => {
      if (messageRes) {
        // console.log(messageRes);
        this.newMessageRes = messageRes; // Use to update optimistic approach in MemberMessagesComponent. Delete the message if api failed.
        this.messagesSig.update(messages => [...messages, messageRes]); // implicit return

        this.scrollToBottom();
      }
    });
  }

  async stopHubConnection(): Promise<void | null> {
    await this.hubConnection?.stop();
  }

  /**
   * This moves scroll to the bottom on the first messages load and any time a new message is sent.
   * Implemented in the service so it applies to the receiver of the message as well when this.hubConnection.on(this._sendMessage is triggered).
   */
  scrollToBottom() {
    try {
      setTimeout(() => {
        if (this.viewport)
          this.viewport.scrollToIndex(this.messagesSig().length - 1, 'smooth');
      }, 0);
    } catch (err) {
      console.error(err)
    }
  }
}

