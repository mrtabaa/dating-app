import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MemberWithRole } from '../models/member-with-role.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl: string = environment.apiUrl + 'admin/';

  getMembersWithRoles(): Observable<MemberWithRole[]> {
    return this.http.get<MemberWithRole[]>(this.apiUrl + 'users-with-roles')
  }

  editMemberRole(username: string, selectedRoles: string[]): Observable<MemberWithRole> {
    const memberWithRoles: MemberWithRole = {
      userName: username,
      roles: selectedRoles
    }

    return this.http.put<MemberWithRole>(this.apiUrl, memberWithRoles);
  }
}
