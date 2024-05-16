import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { PaginationHandler } from '../extensions/paginationHandler';
import { ApiResponseMessage } from '../models/helpers/api-response-message';
import { FollowModifiedEmit } from '../models/helpers/follow-modified-emit';
import { FollowParams } from '../models/helpers/follow-params';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private http = inject(HttpClient);

  private baseUrl = environment.apiUrl + 'follow/';
  private paginationHandler = new PaginationHandler();

  getFollows(followParams: FollowParams): Observable<PaginatedResult<Member[]>> {
    const params = this.getHttpParams(followParams);

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params);
  }

  addFollow(username: string): Observable<ApiResponseMessage> {
    return this.http.post<ApiResponseMessage>(this.baseUrl + username, {});
  }

  removeFollow(username: string): Observable<ApiResponseMessage> {
    return this.http.delete<ApiResponseMessage>(this.baseUrl + username, {});
  }

  modifyFollowUnfollowIcon(members: Member[], followEmit: FollowModifiedEmit): Member[] {
    for (const member of members) {
      if (member === followEmit.member) {
        member.following = followEmit.isFollowing;
      }
    }

    return members;
  }

  //#region Helpers
  private getHttpParams(followParams: FollowParams): HttpParams {
    let params = new HttpParams();

    if (followParams) {
      params = params.append('pageNumber', followParams.pageNumber);
      params = params.append('pageSize', followParams.pageSize);
      params = params.append('predicate', followParams.predicate);
    }

    return params;
  }
  //#endregion
}
