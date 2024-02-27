import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MemberWithRole } from '../../../models/member-with-role.model';
import { AdminService } from '../../../services/admin.service';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-member-management',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule, MatIconModule, MatButtonModule
  ],
  templateUrl: './member-management.component.html',
  styleUrl: './member-management.component.scss'
})
export class MemberManagementComponent {
  private adminService = inject(AdminService);

  displayedColumns = ['no', 'username', 'active-roles', 'action']
  members$: Observable<MemberWithRole[]> | undefined;

  ngOnInit(): void {
    this.getMembersWithRoles();
  }

  getMembersWithRoles(): void {
    this.members$ = this.adminService.getMembersWithRoles();
  }
}
