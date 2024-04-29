import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserWithRole } from '../models/user-with-role.model';
import { ApiResponseMessage } from '../models/helpers/api-response-message';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl: string = environment.apiUrl + 'admin/';

  getMembersWithRoles(): Observable<UserWithRole[]> {
    return this.http.get<UserWithRole[]>(this.apiUrl + 'users-with-roles')
  }

  editMemberRole(username: string, selectedRoles: string[] | null): Observable<string[]> | undefined {
    if (selectedRoles) {
      const memberWithRoles: UserWithRole = {
        userName: username,
        roles: selectedRoles
      }

      return this.http.put<string[]>(this.apiUrl + 'edit-roles', memberWithRoles);
    }

    return undefined;
  }

  deleteMember(userName: string): Observable<ApiResponseMessage> {
    return this.http.delete<ApiResponseMessage>(this.apiUrl + 'delete-member/' + userName);
  }
}
