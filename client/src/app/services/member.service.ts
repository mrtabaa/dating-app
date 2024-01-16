import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { PaginationResult } from '../models/helpers/pagination';
import { Member } from '../models/member.model';
import { environment } from '../../environments/environment';
import { MemberParams } from '../models/helpers/member-params';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);

  baseUrl: string = environment.apiUrl + 'member/';
  paginationResult: PaginationResult<Member[]> = {};

  getMembers(memberParams: MemberParams): Observable<PaginationResult<Member[]>> {
    // if (this.users.length > 0) return of(this.users); // return ObservableOf(users) // caching
    let params = new HttpParams();

    if (memberParams.pageNumber && memberParams.pageSize) {
      params = params.append('pageNumber', memberParams.pageNumber);
      params = params.append('pageSize', memberParams.pageSize);
      params = params.append('gender', memberParams.gender);
      params = params.append('minAge', memberParams.minAge);
      params = params.append('maxAge', memberParams.maxAge);
    }

    return this.http.get<Member[]>(this.baseUrl, { observe: 'response', params }).pipe(
      map(response => {
        if (response.body)
          this.paginationResult.result = response.body // api's response body

        const pagination = response.headers.get('Pagination');
        if (pagination)
          this.paginationResult.pagination = JSON.parse(pagination); // api's response pagination values

        return this.paginationResult;
        // this.users = users; // caching
      })
    );
  }

  getMember(id: string | undefined): Observable<Member> | undefined {
    return this.http.get<Member>(this.baseUrl + 'id/' + id);
  }
}
