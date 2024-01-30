import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, effect, inject } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import { PaginatedResult } from '../models/helpers/pagination';
import { Member } from '../models/member.model';
import { environment } from '../../environments/environment';
import { MemberParams } from '../models/helpers/member-params';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);

  baseUrl: string = environment.apiUrl + 'member/';
  memberCache = new Map<string, PaginatedResult<Member[]>>();
  memberParams: MemberParams | undefined;
  loggedInGender: string | undefined;

  constructor() {
    effect(() => {
      this.loggedInGender = this.accountService.loggedInUserSig()?.gender;

      if (this.loggedInGender) {
        this.memberParams = new MemberParams(this.loggedInGender);

        this.getMembers();
      }
    });
  }

  setMemberParams(memberParamsInput: MemberParams): void {
    this.memberParams = memberParamsInput;
  }

  getMemberParams(): MemberParams | undefined {
    console.log('getParams(', this.memberParams);
    return this.memberParams;
  }

  resetMemberParams(): MemberParams | undefined {
    if (this.loggedInGender)
      return new MemberParams(this.loggedInGender);

    console.log('restet', this.loggedInGender);
    return;
  }

  getMembers(): Observable<PaginatedResult<Member[]>> {

    if (this.memberParams) {
      const response = this.memberCache.get(Object.values(this.memberParams).join('-'));

      if (response) {
        return of(response)
      };
    }

    const params = this.getHttpParams();

    return this.getPaginatedResult<Member[]>(this.baseUrl, params).pipe(
      map(response => {
        if (this.memberParams)
          this.memberCache.set(Object.values(this.memberParams).join('-'), response); // set Value: response in the Key: Object.values(memberParams).join('-')
        return response;
      })
    );
  }

  /**
   * If member is already loaded in memberCache in getMembers(), loop through it and return the member. Otherwise call the api to get the member. 
   * @param idInput 
   * @returns Observable<Member> | undefined
   */
  getMemberById(idInput: string | undefined): Observable<Member> | undefined {
    //#region performance section
    // const member = this.memberCache.get('1-5-18-99-lastActive-male');
    let member: Member | undefined;

    this.memberCache.forEach((value: PaginatedResult<Member[]>) =>
      value.result?.forEach((result: Member) => {
        if (result && result.id === idInput) {
          member = result;
        }
      }
      ));

    if (member) return of(member);
    //#endregion

    // all above code is for performance which prevent calling api if members were already loaded in getMembers()
    return this.http.get<Member>(this.baseUrl + 'id/' + idInput);
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

  /**
   * A reusable pagination method with generic type
   * @param url 
   * @param params 
   * @returns Observable<PaginatedResult<T>>
   */
  getPaginatedResult<T>(url: string, params: HttpParams): Observable<PaginatedResult<T>> {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>;

    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        if (response.body)
          paginatedResult.result = response.body // api's response body

        const pagination = response.headers.get('Pagination');
        if (pagination)
          paginatedResult.pagination = JSON.parse(pagination); // api's response pagination values

        return paginatedResult;
      })
    );
  }
  //#endregion
}
