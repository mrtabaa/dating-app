import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { Member } from '../models/member.model';

@Injectable({
  providedIn: 'root'
})
export class LikeService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'like/';

  addLike(email: string): Observable<object> {
    return this.http.post(this.baseUrl + email, {});
  }

  getLikes(predicate: string): Observable<Member[]> {
    return this.http.get<Member[]>(this.baseUrl + predicate)
  }
}
