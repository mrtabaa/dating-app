import { Component, OnInit, inject } from '@angular/core';
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
export class MemberManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  members$: Observable<MemberWithRole[]> | undefined;
  members!: MemberWithRole[];

  displayedColumns = ['username', 'active-roles', 'action']

  ngOnInit(): void {
    this.getMembersWithRoles();
  }

  getMembersWithRoles(): void {
    this.adminService.getMembersWithRoles().subscribe(
      membersRes => {
        if (membersRes) {
          this.members = membersRes;
          console.log(this.members);
        }
      }
    );
  }
}
