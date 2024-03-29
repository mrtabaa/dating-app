import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, finalize } from 'rxjs';
import { UserUpdate } from '../models/user-update.model';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';
import { ApiResponseMessage } from '../models/helpers/api-response-message';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);

  baseUrl: string = environment.apiUrl + 'user';
  members: Member[] = [];

  updateUser(userUpdate: UserUpdate): Observable<ApiResponseMessage> {
    return this.http.put<ApiResponseMessage>(this.baseUrl, userUpdate)
      .pipe(
        finalize(() => {
          const user = this.members.find(user => user.userName === userUpdate.username);

          if (user) {
            const index = this.members.indexOf(user);
            this.members[index] = { ...this.members[index], ...userUpdate } // copy userUpdate to the list's user
          }
        })
      );
  }

  setMainPhoto(url_128In: string): Observable<ApiResponseMessage> {
    const queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.put<ApiResponseMessage>(this.baseUrl + '/set-main-photo', null, { params: queryParams });
  }

  deletePhoto(url_128In: string): Observable<ApiResponseMessage> {
    const queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.delete<ApiResponseMessage>(this.baseUrl + '/delete-one-photo', { params: queryParams });
  }
}
