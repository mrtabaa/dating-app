import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Message } from '../models/message.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { MessageParams } from '../models/helpers/message-params';
import { MessageIn } from '../models/messageIn.model';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private _http = inject(HttpClient);
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _paginationHandler = new PaginationHandler();

  create(messageIn: MessageIn): Observable<Message> {
    return this._http.post<Message>(this._baseUrl, messageIn);
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
}
