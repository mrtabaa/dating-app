import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpParams } from '@angular/common/http';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { Message } from '../models/message.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { MessageParams } from '../models/helpers/message-params';
import { MessageIn } from '../models/messageIn.model';
import { CreatedMessage } from '../models/createdMessage.model';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { MessagesWithPagination } from '../models/messagesWithPagination.model';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _hubUrl: string = environment.hubUrl + 'message';
  hubConnection: HubConnection | undefined;
  private _paginationHandler = new PaginationHandler();
  createdMessage: CreatedMessage | undefined;
  message: Message | undefined;
  messages: Message[] | undefined;

  paginatedResultSource = new BehaviorSubject<PaginatedResult<Message[]>>({});
  paginatedResult$ = this.paginatedResultSource.asObservable();

  private readonly _messageThread = "MessageThread";
  private readonly _sendMessage = "SendMessage";

  // create(messageIn: MessageIn): Observable<CreatedMessage> {
  //   return this._http.post<CreatedMessage>(this._baseUrl, messageIn);
  // }
  async create(messageIn: MessageIn): Promise<void> {
    this.hubConnection?.invoke<CreatedMessage>('Create', messageIn);
  }

  getInbox(messageParams: MessageParams): void {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    if (messageParams.targetUserName)
      params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    this.paginatedResult$ = this._paginationHandler.getPaginatedResult<Message[]>(this._baseUrl, params);
  }

  createHubConnection(token: string): void {
    // createHubConnection(token: string, messageParams: MessageParams): void {
    // let params = new HttpParams();
    // params = params.append('pageNumber', messageParams.pageNumber);
    // params = params.append('pageSize', messageParams.pageSize);
    // params = params.append('predicate', messageParams.predicate);

    // if (messageParams.targetUserName)
    //   params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    // const queryParams = params.toString();

    this.hubConnection = new HubConnectionBuilder()
      // .withUrl(this._hubUrl + '?' + queryParams, {
      //   accessTokenFactory: () => token
      // })
      .withUrl(this._hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('Hub connection started.'))
      .catch(err => console.log(err));

    // this.hubConnection.on(this._messageThread, messagesWithPagination => {
    //   this.messagesWithPaginationSource.next(messagesWithPagination);
    // })

    this.hubConnection.on(this._sendMessage, (message: Message) => {
      if (message && this.paginatedResult$) {
        this.paginatedResult$.pipe()
        console.log(this.messages);
      }
    });
  }

  stopHubConnection(): void {
    this.hubConnection?.stop();
  }
}

