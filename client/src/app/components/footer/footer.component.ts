import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ResponsiveService } from '../../services/responsive.service';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule
  ],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent {
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;

  currentYear: number = new Date().getFullYear();
}
