import { MatSidenavModule } from '@angular/material/sidenav';
import { Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { ResponsiveService } from '../../../services/responsive.service';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../../services/account.service';

@Component({
  selector: 'app-nav-mobile',
  standalone: true,
  imports: [
    RouterModule, CommonModule,
    MatSidenavModule, MatToolbarModule, MatIconModule, MatButtonModule
  ],
  templateUrl: './nav-mobile.component.html',
  styleUrl: './nav-mobile.component.scss'
})
export class NavMobileComponent {
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;

  /**
     * Set isWelcomeCompSig to true to show WelcomeComponent if brand is clicked. 
     */
  setIsWelcomeCompSig(): void {
    this.isWelcomeCompSig.set(true);
  }
}
