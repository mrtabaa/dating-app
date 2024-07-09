import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Message } from '../models/message.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { MessageParams } from '../models/helpers/message-params';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _paginationHandler = new PaginationHandler();

  getInbox(messageParams: MessageParams): Observable<PaginatedResult<Message[]>> {
    let params = new HttpParams();
    params = params.append('pageNumber', messageParams.pageNumber);
    params = params.append('pageSize', messageParams.pageSize);
    params = params.append('predicate', messageParams.predicate);

    return this._paginationHandler.getPaginatedResult(this._baseUrl, params);
  }
}
