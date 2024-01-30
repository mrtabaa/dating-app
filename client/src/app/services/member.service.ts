import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import { PaginatedResult } from '../models/helpers/pagination';
import { Member } from '../models/member.model';
import { environment } from '../../environments/environment';
import { MemberParams } from '../models/helpers/member-params';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);

  baseUrl: string = environment.apiUrl + 'member/';
  memberCache = new Map<string, PaginatedResult<Member[]>>();

  getMembers(memberParams: MemberParams): Observable<PaginatedResult<Member[]>> {

    const response = this.memberCache.get(Object.values(memberParams).join('-'));

    if (response) return of(response);

    const params = this.getHttpParams(memberParams);

    return this.getPaginatedResult<Member[]>(this.baseUrl, params).pipe(
      map(response => {
        this.memberCache.set(Object.values(memberParams).join('-'), response); // set Value: response in the Key: Object.values(memberParams).join('-')
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
  private getHttpParams(memberParams: MemberParams): HttpParams {
    let params = new HttpParams();

    params = params.append('pageNumber', memberParams.pageNumber);
    params = params.append('pageSize', memberParams.pageSize);
    params = params.append('gender', memberParams.gender);
    params = params.append('minAge', memberParams.minAge);
    params = params.append('maxAge', memberParams.maxAge);
    params = params.append('orderBy', memberParams.orderBy);

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
