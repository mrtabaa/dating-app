import { Component, OnInit, inject } from '@angular/core';
import { Observable, take, tap } from 'rxjs';
import { MemberWithRole } from '../../../models/member-with-role.model';
import { AdminService } from '../../../services/admin.service';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-member-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatTableModule, MatFormFieldModule, MatSelectModule, MatButtonModule
  ],
  templateUrl: './member-management.component.html',
  styleUrl: './member-management.component.scss'
})
export class MemberManagementComponent implements OnInit{
  private adminService = inject(AdminService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  displayedColumns = ['no', 'username', 'active-roles', 'action'];
  displayedRoles = ['admin', 'moderator', 'member'];

  membersWithRole$: Observable<MemberWithRole[]> | undefined;

  selectedArray = this.fb.array<string[] | null>([]);

  ngOnInit(): void {
    this.getMembersWithRoles();
  }

  getMembersWithRoles(): void {
    this.membersWithRole$ = this.adminService.getMembersWithRoles()
      .pipe(tap(members => members
        .forEach(member => {
          member.roles
          this.selectedArray.push(this.fb.control<string[]>(member.roles, [Validators.required]));
        })));
  }

  updateRoles(i: number, userName: string): void {

    this.adminService.editMemberRole(userName, this.selectedArray.at(i).value)
      ?.pipe(
        take(1)
      ).subscribe(
        {
          next: savedRoles => {
            if (savedRoles) {
              this.selectedArray.at(i).setValue(savedRoles);

              this.selectedArray.at(i).markAsPristine(); // disable the Save button

              this.snackBar.open('The ' + userName + "'s new roles are saved successfully.", "Close", { horizontalPosition: "center", verticalPosition: "bottom", duration: 7000 });
            }
          },
        });
  }
}
