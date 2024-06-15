import { Component, inject } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { HasRoleDirective } from '../../../directives/has-role.directive';
import { MemberManagementComponent } from '../member-management/member-management.component';
import { PhotoManagementComponent } from '../photo-management/photo-management.component';
import { ResponsiveService } from '../../../services/responsive.service';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [
    HasRoleDirective,
    MatTabsModule,
    MemberManagementComponent, PhotoManagementComponent
  ],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.scss'
})
export class AdminPanelComponent {
isMobileSig = inject(ResponsiveService).isMobileSig;
}
