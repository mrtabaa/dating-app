import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import { PaginationResult } from '../models/helpers/pagination';
import { Member } from '../models/member.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);

  baseUrl: string = environment.apiUrl + 'member/';
  paginationResult: PaginationResult<Member[]> = {};

  getMembers(page?: number, itemsPerPage?: number): Observable<PaginationResult<Member[]>> {
    // if (this.users.length > 0) return of(this.users); // return ObservableOf(users) // caching
    let params = new HttpParams();

    if (page && itemsPerPage) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
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
