import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { user } from '../models/user.model';
import { Observable, finalize, map, of, take } from 'rxjs';
import { MemberUpdate } from '../models/user-update.model';
import { UpdateResult } from '../models/helpers/update-result.model';
import { AccountService } from './account.service';
import { User } from '../models/user.model';
import { PaginationResult } from '../models/helpers/pagination';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  baseUrl: string = environment.apiUrl + 'user';
  members: user[] = [];
  paginationResult: PaginationResult<user[]> = {};
  user: user | undefined;
  user!: User;

  constructor(private http: HttpClient, accountService: AccountService) {
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => { if (user) this.user = user; }
    });
  }

  getMembers(page?: number, itemsPerPage?: number): Observable<PaginationResult<user[]>> {
    // if (this.members.length > 0) return of(this.members); // return ObservableOf(members) // caching
    let params = new HttpParams();

    if (page && itemsPerPage) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    return this.http.get<user[]>(this.baseUrl, { observe: 'response', params }).pipe(
      map(response => {
        if (response.body)
          this.paginationResult.result = response.body // api's response body

        const pagination = response.headers.get('Pagination');
        if (pagination)
          this.paginationResult.pagination = JSON.parse(pagination); // api's response pagination values

        return this.paginationResult;
        // this.members = members; // caching
      })
    );
  }

  getMember(email: string): Observable<user> {
    if (this.members.length > 0) {
      this.user = this.members.find(user => user.email === email);
      if (this.user)
        return of(this.user);
    }

    return this.http.get<user>(this.baseUrl + '/email/' + email);
  }

  updateMember(memberUpdate: MemberUpdate): Observable<UpdateResult> {
    return this.http.put<UpdateResult>(this.baseUrl, memberUpdate).pipe(
      finalize(() => {
        const user = this.members.find(user => user.email === memberUpdate.email);
        if (user) {
          const index = this.members.indexOf(user);
          this.members[index] = { ...this.members[index], ...memberUpdate } // copy memberUpdate to the list's user
        }
      })
    );
  }

  setMainPhoto(url_128In: string): Observable<UpdateResult> {
    let queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.put<UpdateResult>(this.baseUrl + '/set-main-photo', null, { params: queryParams });
  }

  deletePhoto(url_128In: string): Observable<UpdateResult> {
    let queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.delete<UpdateResult>(this.baseUrl + '/delete-one-photo', { params: queryParams });
  }
}
