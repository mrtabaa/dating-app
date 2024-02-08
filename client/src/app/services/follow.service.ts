import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Member } from '../models/member.model';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'follow/';

  addFollow(email: string): Observable<object> {
    return this.http.post(this.baseUrl + email, {});
  }

  getLikes(predicate: string): Observable<Member[]> {
    return this.http.get<Member[]>(this.baseUrl + predicate)
  }
}
