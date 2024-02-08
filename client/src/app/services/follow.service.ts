import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';
import { PaginatedResult } from '../models/helpers/pagination';
import { PaginationHandler } from '../extensions/paginationHandler';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private http = inject(HttpClient);

  private baseUrl = environment.apiUrl + 'follow/';
  private paginationHandler = new PaginationHandler();

  addFollow(email: string): Observable<object> {
    return this.http.post(this.baseUrl + email, {});
  }

  getFollows(predicate: string): Observable<PaginatedResult<Member[]>> {
    let params = new HttpParams();
    params = params.append('predicate', predicate);

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params);
  }
}
