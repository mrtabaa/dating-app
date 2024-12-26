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
import {AccountService} from "./account.service";
import {SignalRMessages} from "../extensions/signalRMessages";

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  hubConnection: HubConnection | undefined;
  newMessageRes: Message | undefined;
  messagesSig = signal<Message[]>([]);
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

  async createHubConnectionAsync(token: string, targetUsername: string): Promise<void> {
    this.targetUserName = targetUsername;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start()
      .then(() => console.log('MessageHub connection started.'));

    await this.joinGroupAsync();

    this.getUpdatedReadOn();
    this.getNewMessageRes();
  }

  // TODO: Implement delete message.
  async joinGroupAsync(): Promise<void> {
    await this.hubConnection?.invoke(SignalRMessages.JoinGroup, this.targetUserName)
      .then(() => console.log(this._loggedInUserSig()?.userName, 'joined the chat.'))
      .catch(err => console.log(this._loggedInUserSig()?.userName, 'failed to join the chat with error:', err));
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
      }
    });
  }

  async stopHubConnectionAsync(): Promise<void | null> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      // await this.leaveGroupAsync();
      await this.hubConnection?.stop()
        .then(() => console.log(this._loggedInUserSig()?.userName, 'left the group.'))
        .catch(err => console.log(this._loggedInUserSig()?.userName, 'failed to leave the chat with error:', err));
    }
  }
}
