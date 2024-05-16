import { Component, OnInit, inject } from '@angular/core';
import { take } from 'rxjs';
import { UserWithRole } from '../../../models/user-with-role.model';
import { AdminService } from '../../../services/admin.service';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { BaseParams } from '../../../models/helpers/BaseParams';
import { Pagination } from '../../../models/helpers/pagination';
import { PaginatedResult } from '../../../models/helpers/paginatedResult';

@Component({
  selector: 'app-member-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatTableModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatIconModule, MatPaginatorModule
  ],
  templateUrl: './member-management.component.html',
  styleUrl: './member-management.component.scss'
})
export class MemberManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  displayedColumns = ['no', 'username', 'edit-role', 'active-roles', 'delete-member'];
  displayedRoles = ['admin', 'moderator', 'member'];

  usersWithRole: UserWithRole[] | undefined;

  selectedRoles = this.fb.array<string[] | null>([]);

  pagination: Pagination | undefined;
  pageSizeOptions = [5, 10, 25];
  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;
  pageEvent: PageEvent | undefined;
  baseParams: BaseParams | undefined;

  ngOnInit(): void {
    this.baseParams = new BaseParams();

    this.getMembersWithRoles();
  }

  getMembersWithRoles(): void {
    if (this.baseParams)
      this.adminService.getMembersWithRoles(this.baseParams)
        .pipe(
          take(1)
        ).subscribe({
          next: (response: PaginatedResult<UserWithRole[]>) => {
            if (response.result && response.pagination) {
              console.log('hit');
              response.result.forEach(user => {
                this.selectedRoles.push(this.fb.control<string[]>(user.roles, [Validators.required]));
              });

              this.usersWithRole = response.result;
              this.pagination = response.pagination;

              console.log(response.result);
            }
          }
        });
  }

  updateRoles(i: number, userName: string): void {
    this.adminService.editMemberRole(userName, this.selectedRoles.at(i).value)?.pipe(
      take(1)
    ).subscribe({
      next: (savedRoles: string[]) => {
        if (savedRoles) {
          this.selectedRoles.at(i).setValue(savedRoles);

          this.selectedRoles.at(i).markAsPristine(); // disable the Save button

          this.snackBar.open('The ' + userName + "'s new roles are saved successfully.", "Close", { horizontalPosition: "center", verticalPosition: "bottom", duration: 7000 });
        }
      }
    });
  }

  deleteMember(i: number, userName: string): void {
    this.adminService.deleteMember(userName).pipe(
      take(1)
    ).subscribe({
      next: (response: ApiResponseMessage) => {
        this.snackBar.open(response.message, "Close", { horizontalPosition: "center", verticalPosition: "bottom", duration: 7000 });

        // Slice and copy the array to trigger the change detection to update the mat-table
        if (this.usersWithRole)
          this.usersWithRole = [
            ...this.usersWithRole.slice(0, i),
            ...this.usersWithRole.slice(i + 1)
          ];
      }
    });
  }

  handlePageEvent(e: PageEvent) {
    if (this.baseParams) {
      if (e.pageSize !== this.baseParams.pageSize)
        e.pageIndex = 0;

      this.pageEvent = e;
      this.baseParams.pageSize = e.pageSize;
      this.baseParams.pageNumber = e.pageIndex + 1;

      this.getMembersWithRoles();
    }
  }
}
