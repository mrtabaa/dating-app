import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import { PaginatedResult } from "../models/helpers/paginatedResult";
import { Member } from '../models/member.model';
import { environment } from '../../environments/environment';
import { MemberParams } from '../models/helpers/member-params';
import { AccountService } from './account.service';
import { PaginationHandler } from '../extensions/paginationHandler';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);

  private paginationHandler = new PaginationHandler();
  baseUrl: string = environment.apiUrl + 'member/';
  memberCache = new Map<string, PaginatedResult<Member[]>>();
  memberParams: MemberParams | undefined;

  constructor() {
    this.getFreshMemberParams();
  }

  setMemberParams(memberParamsInput: MemberParams): void {
    this.memberParams = memberParamsInput;
  }

  getFreshMemberParams(): MemberParams | undefined {
    // retrieve gender from localStorage since accountService.loggedInUserSig is ran after this service and gender will be undefined.
    const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const gender: string = JSON.parse(loggedInUserStr).gender;

      if (gender) {
        this.memberParams = new MemberParams(gender);
        this.getMembers();
      }
      else { // for admin who doesn't have a gender
        this.memberParams = new MemberParams('male');
        this.getMembers();
      }
    }

    return this.memberParams;
  }

  getMembers(): Observable<PaginatedResult<Member[]>> {
    let response: PaginatedResult<Member[]> | undefined;

    if (this.memberParams)
      response = this.memberCache.get(Object.values(this.memberParams).join('-'));

    if (response) return of(response);

    const params = this.getHttpParams();

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params)
      .pipe(
        map(response => {
          if (this.memberParams)
            this.memberCache.set(Object.values(this.memberParams).join('-'), response); // set Value: response in the Key: Object.values(memberParams).join('-')

          return response;
        })
      );
  }

  /**
   * If member is already loaded in memberCache in getMembers(), loop through it and return the member. Otherwise call the api to get the member. 
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
  private getHttpParams(): HttpParams {
    let params = new HttpParams();

    if (this.memberParams) {
      params = params.append('pageNumber', this.memberParams.pageNumber);
      params = params.append('pageSize', this.memberParams.pageSize);
      params = params.append('gender', this.memberParams.gender);
      params = params.append('minAge', this.memberParams.minAge);
      params = params.append('maxAge', this.memberParams.maxAge);
      params = params.append('orderBy', this.memberParams.orderBy);
    }

    return params;
  }
  //#endregion
}
