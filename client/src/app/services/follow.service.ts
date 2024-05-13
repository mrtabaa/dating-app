import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { FollowPredicate } from '../models/helpers/follow-predicate';
import { ApiResponseMessage } from '../models/helpers/api-response-message';
import { FollowModifiedEmit } from '../models/helpers/follow-modified-emit';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private http = inject(HttpClient);

  private baseUrl = environment.apiUrl + 'follow/';
  private paginationHandler = new PaginationHandler();

  addFollow(username: string): Observable<ApiResponseMessage> {
    return this.http.post<ApiResponseMessage>(this.baseUrl + username, {});
  }

  removeFollow(username: string): Observable<ApiResponseMessage> {
    return this.http.delete<ApiResponseMessage>(this.baseUrl + username, {});
  }

  getFollows(predicate: FollowPredicate): Observable<PaginatedResult<Member[]>> {
    let params = new HttpParams();
    params = params.append('predicate', predicate);

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params);
  }

  modifyFollowUnfollowIcon(members: Member[], followEmit: FollowModifiedEmit): Member[] {
    for (const member of members) {
      if (member === followEmit.member) {
        member.following = followEmit.isFollowing;
      }
    }

    return members;
  }
}
