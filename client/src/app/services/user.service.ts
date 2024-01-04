import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, finalize } from 'rxjs';
import { UserUpdate } from '../models/user-update.model';
import { UpdateResult } from '../models/helpers/update-result.model';
import { PaginationResult } from '../models/helpers/pagination';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);

  baseUrl: string = environment.apiUrl + 'user';
  members: Member[] = [];
  paginationResult: PaginationResult<Member[]> = {};

  updateUser(userUpdate: UserUpdate): Observable<UpdateResult> {
    return this.http.put<UpdateResult>(this.baseUrl, userUpdate).pipe(
      finalize(() => {
        const user = this.members.find(user => user.email === userUpdate.email);
        if (user) {
          const index = this.members.indexOf(user);
          this.members[index] = { ...this.members[index], ...userUpdate } // copy userUpdate to the list's user
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
