import { Component, OnInit, Signal, inject } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { LoggedInUser } from '../../models/logged-in-user.model';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule, RouterLink, RouterLinkActive, NgOptimizedImage,
    MatIconModule, MatToolbarModule, MatTabsModule, MatMenuModule,
    MatButtonModule, MatListModule, MatDividerModule
  ],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  private accountService = inject(AccountService);

  basePhotoUrl = environment.apiPhotoUrl;

  loggedInUserSig: Signal<LoggedInUser | null> | undefined;

  links = ['members', 'friends', 'messages', 'admin'];

  ngOnInit(): void {
    this.loggedInUserSig = this.accountService.loggedInUserSig;
  }

  logout() {
    this.accountService.logout();
  }
}
