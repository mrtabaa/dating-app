import {inject, Injectable, signal} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Message} from '../models/message.model';
import {PaginatedResult} from '../models/helpers/paginatedResult';
import {PaginationHandler} from '../extensions/paginationHandler';
import {MessageParams} from '../models/helpers/message-params';
import {MessageIn} from '../models/messageIn.model';
import {HubConnection, HubConnectionBuilder, HubConnectionState} from '@microsoft/signalr';
import {CdkVirtualScrollViewport} from '@angular/cdk/scrolling';
import {AccountService} from "./account.service";
import {SignalRMessages} from "../extensions/signalRMessages";

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

  async createHubConnectionAsync(token: string): Promise<void> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start()
      .then(() => console.log('MessageHub connection started.'));

    await this.joinGroupAsync()
      .then(() => console.log(this._loggedInUserSig()?.userName, 'joined the chat.'))
      .catch(err => console.log(this._loggedInUserSig()?.userName, 'failed to joined the chat with error:', err));

    this.getUpdatedReadOn();
    this.getNewMessageRes();
  }

  // TODO If both parties are online, mark created messages as Read.
  // TODO Implement delete message.
  async joinGroupAsync(): Promise<void> {
    await this.hubConnection?.invoke(SignalRMessages.JoinGroup, this.targetUserName);
  }

  getUpdatedReadOn(): void {
    this.hubConnection?.off(SignalRMessages.UpdatedReadOn); // Remove existing listener to prevent memory leak
    this.hubConnection?.on(SignalRMessages.UpdatedReadOn, (updatedReadOn: Date): void => {
      const readOnDate = new Date(updatedReadOn); // convert SignalR ISO string to Date object

      this.messagesSig.update(messages => // implicit return
        messages.map(message => ({
          ...message,
          readOn: readOnDate
        }))
      )
    });
  }

  async createAsync(messageIn: MessageIn): Promise<void> {
    this.newMessageRes = undefined; // reset each time a new message is sent. Set value at this.hubConnection.on(this._sendMessage
    await this.hubConnection?.invoke(SignalRMessages.Create, messageIn);
  }

  getNewMessageRes(): void {
    this.hubConnection?.off(SignalRMessages.NewMessageRes); // Remove existing listener to prevent memory leak
    this.hubConnection?.on(SignalRMessages.NewMessageRes, (messageRes: Message) => {
      if (messageRes) {
        // console.log(messageRes);
        this.newMessageRes = messageRes; // Use to update optimistic approach in MemberMessagesComponent. Delete the message if api failed.
        this.messagesSig.update(messages => [...messages, messageRes]); // implicit return

        this.scrollToBottom();
      }
    });
  }

  async leaveGroupAsync(): Promise<void> {
    await this.hubConnection?.invoke(SignalRMessages.LeaveGroup, this.targetUserName)
      .then(() => console.log(this._loggedInUserSig()?.userName, 'left the group.'))
      .catch(err => console.log(err));
  }

  async stopHubConnectionAsync(): Promise<void | null> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.leaveGroupAsync();
      await this.hubConnection?.stop();
    }
  }

  /**
   * This moves scroll to the bottom on the first messages load and any time a new message is sent.
   * Implemented in the service so it applies to the receiver of the message as well when this.hubConnection.on(this._sendMessage is triggered).
   */
  scrollToBottom(): void {
    try {
      setTimeout((): void => {
        if (this.viewport)
          this.viewport.scrollToIndex(this.messagesSig().length - 1, 'smooth');
      }, 0);
    } catch (err) {
      console.error(err)
    }
  }
}
