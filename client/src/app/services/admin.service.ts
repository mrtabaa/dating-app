import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserWithRole } from '../models/user-with-role.model';
import { ApiResponseMessage } from '../models/helpers/api-response-message';
import { PaginationHandler } from '../extensions/paginationHandler';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { AdminParams } from '../models/helpers/AdminParams';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl: string = environment.apiUrl + 'admin/';
  private paginationHandler = new PaginationHandler();

  getMembersWithRoles(adminParams: AdminParams): Observable<PaginatedResult<UserWithRole[]>> {
    const params = this.getHttpParams(adminParams);

    return this.paginationHandler.getPaginatedResult<UserWithRole[]>(this.apiUrl + 'users-with-roles', params);
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

  private getHttpParams(adminParams: AdminParams): HttpParams {
    let params = new HttpParams();

    params = params.append('pageNumber', adminParams.pageNumber);
    params = params.append('pageSize', adminParams.pageSize);

    if (adminParams.search)
      params = params.append('search', adminParams.search);

    return params;
  }
}
