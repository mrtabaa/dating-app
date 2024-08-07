import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
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
  private _http = inject(HttpClient);
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _hubUrl: string = environment.hubUrl + 'message';
  private _hubConnection: HubConnection | undefined;
  private _paginationHandler = new PaginationHandler();
  // messagesWithPaginationSig = signal<MessagesWithPagination>({
  //   messages: [],
  //   pagination: {
  //     currentPage: 0,
  //     itemsPerPage: 0,
  //     totalItems: 0,
  //     totalPages: 0,
  //   }
  // });

  messagesWithPaginationSource = new BehaviorSubject<MessagesWithPagination>({
    messages: [],
    pagination: {
      currentPage: 0,
      itemsPerPage: 0,
      totalItems: 0,
      totalPages: 0,
    }
  });

  messagesWithPagination$ = this.messagesWithPaginationSource.asObservable();

  private readonly _messageThread = "MessageThread";

  create(messageIn: MessageIn): Observable<CreatedMessage> {
    return this._http.post<CreatedMessage>(this._baseUrl, messageIn);
  }

  getInbox(messageParams: MessageParams): Observable<PaginatedResult<Message[]>> {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    if (messageParams.targetUserName)
      params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    return this._paginationHandler.getPaginatedResult(this._baseUrl, params);
  }

  createHubConnection(token: string, messageParams: MessageParams): void {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    if (messageParams.targetUserName)
      params = params.append('targetUserName', messageParams.targetUserName); // Set for THREAD

    const queryParams = params.toString();

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl + '?' + queryParams, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this._hubConnection.start().catch(err => console.log(err));

    this._hubConnection.on(this._messageThread, messagesWithPagination => {
      this.messagesWithPaginationSource.next(messagesWithPagination);
    })
  }
}
