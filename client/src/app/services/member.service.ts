import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member.model';
import { Observable } from 'rxjs';
import { MemberUpdate } from '../models/member-update.model';
import { UpdateResult } from '../models/helpers/update-result.model';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  baseUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMembers(): Observable<Member[]> {
    return this.http.get<Member[]>(this.baseUrl + 'user');
  }

  getMember(email: string): Observable<Member> {
    return this.http.get<Member>(this.baseUrl + 'user/email/' + email);
  }

  updateMember(memberUpdate: MemberUpdate): Observable<UpdateResult> {
    return this.http.put<any>(this.baseUrl + 'user/update', memberUpdate);
  }
}
