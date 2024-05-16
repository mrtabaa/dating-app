import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserWithRole } from '../models/user-with-role.model';
import { ApiResponseMessage } from '../models/helpers/api-response-message';
import { BaseParams } from '../models/helpers/BaseParams';
import { PaginationHandler } from '../extensions/paginationHandler';
import { PaginatedResult } from '../models/helpers/paginatedResult';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl: string = environment.apiUrl + 'admin/';
  private paginationHandler = new PaginationHandler();

  getMembersWithRoles(baseParams: BaseParams): Observable<PaginatedResult<UserWithRole[]>> {
    const params = this.getHttpParams(baseParams);

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

  private getHttpParams(baseParams: BaseParams): HttpParams {
    let params = new HttpParams();

    params = params.append('pageNumber', baseParams.pageNumber);
    params = params.append('pageSize', baseParams.pageSize);

    return params;
  }
}
