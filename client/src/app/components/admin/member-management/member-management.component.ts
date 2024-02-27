import { Component, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { MemberWithRole } from '../../../models/member-with-role.model';
import { AdminService } from '../../../services/admin.service';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

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
export class MemberManagementComponent {
  private adminService = inject(AdminService);
  private fb = inject(FormBuilder);

  displayedColumns = ['no', 'username', 'active-roles', 'action'];
  displayedRoles = ['admin', 'moderator', 'member'];

  members$: Observable<MemberWithRole[]> | undefined;

  selectedArray = this.fb.array([]);

  ngOnInit(): void {
    this.getMembersWithRoles();
  }

  getMembersWithRoles(): void {
    this.members$ = this.adminService.getMembersWithRoles()
      .pipe(tap(members => members
        .forEach(member => {
          member.roles
          this.selectedArray.push(this.fb.control(member.roles));
        })));
  }

  updateRoles(i: number): void {
    console.log(this.selectedArray.at(i).value);
  }
}
