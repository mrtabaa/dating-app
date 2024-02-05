import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LikeService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'like/';

  addLike(email: string): Observable<object> {
    return this.http.post(this.baseUrl + email, {});
  }
}
