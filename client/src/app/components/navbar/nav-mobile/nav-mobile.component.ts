import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { Component, ViewChild, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterModule } from '@angular/router';
import { ResponsiveService } from '../../../services/responsive.service';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { AccountService } from '../../../services/account.service';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-nav-mobile',
  standalone: true,
  imports: [
    RouterModule, CommonModule, NgOptimizedImage,
    MatSidenavModule, MatToolbarModule, MatIconModule, MatButtonModule,
    MatListModule, MatDividerModule
  ],
  templateUrl: './nav-mobile.component.html',
  styleUrl: './nav-mobile.component.scss'
})
export class NavMobileComponent {
  @ViewChild('drawer') drawer: MatDrawer | undefined;

  private router = inject(Router);
  private accountService = inject(AccountService);
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;

  /**
     * Set isWelcomeCompSig to true to show WelcomeComponent if brand is clicked. 
     */
  setIsWelcomeCompSig(): void {
    if (!this.loggedInUserSig())
      this.isWelcomeCompSig.set(true); // change nav-mobile bg-color: transparent & color: white
  }

  closeDrawar(): void {
    this.drawer?.close();
  }

  logout() {
    this.accountService.logout();

    if (this.drawer)
      this.drawer.close();
  }
}
