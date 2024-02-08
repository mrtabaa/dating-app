import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, effect, inject } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import { PaginatedResult } from '../models/helpers/pagination';
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
  loggedInGender: string | undefined;

  constructor() {
    effect(() => {
      const gender = this.accountService.loggedInUserSig()?.gender;

      if (gender) {
        this.memberParams = new MemberParams(gender);
        this.loggedInGender = gender;

        localStorage.setItem('memberParams', JSON.stringify(this.memberParams));

        this.getMembers(); // since it relies on memberParams and loggedInUserSig()?.gender, it has to be called in effect. 
      }
    });
  }

  setMemberParams(memberParamsInput: MemberParams): void {
    this.memberParams = memberParamsInput;
  }

  getMemberParams(): MemberParams | undefined {
    return this.memberParams;
  }

  resetMemberParams(): MemberParams | undefined {
    if (this.loggedInGender)
      return new MemberParams(this.loggedInGender);

    return;
  }

  getMembers(): Observable<PaginatedResult<Member[]>> {
    let response: PaginatedResult<Member[]> | undefined;
    // console.log('getMembers()', this.memberParams);

    if (this.memberParams)
      response = this.memberCache.get(Object.values(this.memberParams).join('-'));

    if (response) return of(response);

    const params = this.getHttpParams();

    return this.paginationHandler.getPaginatedResult<Member[]>(this.baseUrl, params).pipe(
      map(response => {
        if (this.memberParams)
          this.memberCache.set(Object.values(this.memberParams).join('-'), response); // set Value: response in the Key: Object.values(memberParams).join('-')
        return response;
      })
    );
  }

  /**
   * If member is already loaded in memberCache in getMembers(), loop through it and return the member. Otherwise call the api to get the member. 
   * @param emailInput 
   * @returns Observable<Member> | undefined
   */
  getMemberByEmail(emailInput: string | undefined): Observable<Member> | undefined {
    //#region performance section
    // const member = this.memberCache.get('1-5-18-99-lastActive-male');
    let member: Member | undefined;

    this.memberCache.forEach((value: PaginatedResult<Member[]>) =>
      value.result?.forEach((result: Member) => {
        if (result && result.email === emailInput) {
          member = result;
        }
      }
      ));

    if (member) return of(member);
    //#endregion

    // all above code is for performance which prevent calling api if members were already loaded in getMembers()
    return this.http.get<Member>(this.baseUrl + 'email/' + emailInput);
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
