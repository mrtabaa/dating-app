import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, finalize, map, of, take } from 'rxjs';
import { UserUpdate } from '../models/user-update.model';
import { UpdateResult } from '../models/helpers/update-result.model';
import { AccountService } from './account.service';
import { LoggedInUser } from '../models/loggedInUser.model';
import { PaginationResult } from '../models/helpers/pagination';
import { environment } from '../../environments/environment';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl: string = environment.apiUrl + 'user';
  users: User[] = [];
  paginationResult: PaginationResult<User[]> = {};
  user: User | undefined;
  loggedInUser!: LoggedInUser;

  constructor(private http: HttpClient, accountService: AccountService) {
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: loggedInUser => { if (loggedInUser) this.loggedInUser = loggedInUser; }
    });
  }

  getUsers(page?: number, itemsPerPage?: number): Observable<PaginationResult<User[]>> {
    // if (this.users.length > 0) return of(this.users); // return ObservableOf(users) // caching
    let params = new HttpParams();

    if (page && itemsPerPage) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    return this.http.get<User[]>(this.baseUrl, { observe: 'response', params }).pipe(
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

  getUser(email: string): Observable<User> {
    if (this.users.length > 0) {
      this.user = this.users.find(user => user.email === email);
      if (this.user)
        return of(this.user);
    }

    return this.http.get<User>(this.baseUrl + '/email/' + email);
  }

  updateUser(userUpdate: UserUpdate): Observable<UpdateResult> {
    return this.http.put<UpdateResult>(this.baseUrl, userUpdate).pipe(
      finalize(() => {
        const user = this.users.find(user => user.email === userUpdate.email);
        if (user) {
          const index = this.users.indexOf(user);
          this.users[index] = { ...this.users[index], ...userUpdate } // copy userUpdate to the list's user
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
