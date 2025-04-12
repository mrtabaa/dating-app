import {HttpClient, HttpParams} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {UserUpdate} from '../models/user-update.model';
import {environment} from '../../environments/environment';
import {ApiResponseMessage} from '../models/helpers/api-response-message';
import {PhotoDeleteResponse} from '../models/helpers/photo-delete-response';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);

  private baseUrl: string = environment.apiUrl + 'user';

  updateUser(userUpdate: UserUpdate): Observable<ApiResponseMessage> {
    return this.http.put<ApiResponseMessage>(this.baseUrl, userUpdate);
  }

  getAllPhotos(): Observable<string[]> {
    return this.http.get<string[]>(this.baseUrl + '/get-all-photos');
  }

  setMainPhoto(url_128In: string): Observable<ApiResponseMessage> {
    const queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.put<ApiResponseMessage>(this.baseUrl + '/set-main-photo', null, {params: queryParams});
  }

  deletePhoto(url_128In: string): Observable<PhotoDeleteResponse> {
    const queryParams = new HttpParams().set('photoUrlIn', url_128In);

    return this.http.delete<PhotoDeleteResponse>(this.baseUrl + '/delete-one-photo', {params: queryParams});
  }
}
