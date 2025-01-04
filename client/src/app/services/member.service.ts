import {HttpClient, HttpParams} from '@angular/common/http';
import {EventEmitter, inject, Injectable} from '@angular/core';
import {map, Observable, of} from 'rxjs';
import {PaginatedResult} from "../models/helpers/paginatedResult";
import {Member} from '../models/member.model';
import {environment} from '../../environments/environment';
import {MemberParams} from '../models/helpers/member-params';
import {PaginationHandler} from '../extensions/paginationHandler';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  //#region Mobile
  baseUrl: string = environment.apiUrl + 'member/';
  memberCache = new Map<string, PaginatedResult<Member[]>>();
  eventEmitOrderFilterBottomSheet = new EventEmitter<void>();
  memberParams: MemberParams | undefined;
  //#endregion
  paginatedResultMembers: PaginatedResult<Member[]> | undefined;

  //#region OrderBottomSheetComponent and FilterBottomSheetComponent signal/vars values
  private http = inject(HttpClient);
  //#endregion OrderBottomSheetComponent and FilterBottomSheetComponent signal/vars values

  private paginationHandler = new PaginationHandler();

  getMembers(memberParams: MemberParams): Observable<PaginatedResult<Member[]>> {
    let response: PaginatedResult<Member[]> | undefined;

    if (memberParams)
      response = this.memberCache.get(Object.values(memberParams).join('-'));

    if (response) return of(response);

    const params = this.getHttpParams(memberParams);

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params)
      .pipe(
        map(response => {
          this.paginatedResultMembers = response;

          if (memberParams) {
            this.memberParams = memberParams; // to use in resetMembersAfterFollowModified()
            this.memberCache.set(Object.values(memberParams).join('-'), response); // set Value: response in the Key: Object.values(memberParams).join('-')
          }

          return response;
        })
      );
  }

  resetMembersAfterFollowModified(userName: string, isFollowing: boolean): void {
    if (this.memberParams && this.paginatedResultMembers?.result) {
      for (const member of this.paginatedResultMembers.result) {
        if (member.userName === userName)
          member.isFollowing = isFollowing;
      }

      this.memberCache.set(Object.values(this.memberParams).join('-'), this.paginatedResultMembers)
    }
  }

  /**
   * If member is already loaded in memberCache in getMembers(), loop through it and return the member. Otherwise, call the api to get the member.
   * @param usernameIn
   * @returns Observable<Member> | undefined
   */
  getMemberByUsername(usernameIn: string | undefined): Observable<Member> | undefined {
    //#region performance section
    // const member = this.memberCache.get('1-5-18-99-lastActive-male');
    let member: Member | undefined;

    this.memberCache.forEach((value: PaginatedResult<Member[]>) =>
      value.result?.forEach((result: Member) => {
          if (result && result.userName === usernameIn) {
            member = result;
          }
        }
      ));

    if (member) return of(member);
    //#endregion

    // all above code is for performance which prevent calling api if members were already loaded in getMembers()
    return this.http.get<Member>(this.baseUrl + 'username/' + usernameIn);
  }

  //#region Helpers
  private getHttpParams(memberParams: MemberParams): HttpParams {
    let params = new HttpParams();

    if (memberParams) {
      if (memberParams.userNameOrKnownAs)
        params = params.append('userNameOrKnownAs', memberParams.userNameOrKnownAs);
      if (memberParams.gender)
        params = params.append('gender', memberParams.gender);

      params = params.append('pageNumber', memberParams.pageNumber);
      params = params.append('pageSize', memberParams.pageSize);
      params = params.append('minAge', memberParams.minAge);
      params = params.append('maxAge', memberParams.maxAge);
      params = params.append('orderBy', memberParams.orderBy);
    }

    return params;
  }

  //#endregion
}
