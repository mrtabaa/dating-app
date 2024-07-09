import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Message } from '../models/message.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { PaginationParams } from '../models/helpers/paginationParams';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private _baseUrl: string = environment.apiUrl + 'message/';
  private _paginationHandler = new PaginationHandler();

  getInboxMessages(pageParams: PaginationParams): Observable<PaginatedResult<Message[]>> {
    let params = new HttpParams();
    params = params.append('pageNumber', pageParams.pageNumber);
    params = params.append('pageSize', pageParams.pageSize);

    this._paginationHandler.getPaginatedResult(this._baseUrl + 'inbox', params).subscribe(res => console.log(res));
    return this._paginationHandler.getPaginatedResult(this._baseUrl + 'inbox', params);
  }
}
