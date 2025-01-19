import {Component, inject} from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {AccountService} from '../../services/account.service';
import {Router, RouterLink, RouterLinkActive} from '@angular/router';
import {MatIconModule} from '@angular/material/icon';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatTabsModule} from '@angular/material/tabs';
import {MatMenuModule} from '@angular/material/menu';
import {MatButtonModule} from '@angular/material/button';
import {MatListModule} from '@angular/material/list';
import {MatDividerModule} from '@angular/material/divider';
import {CommonService} from "../../services/common.service";

@Component({
  selector: 'app-navbar',
  imports: [
    CommonModule, RouterLink, RouterLinkActive, NgOptimizedImage,
    MatIconModule, MatToolbarModule, MatTabsModule, MatMenuModule,
    MatButtonModule, MatListModule, MatDividerModule
  ],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isResetPasswordCompSig = inject(CommonService).isResetPasswordCompSig;
  links = ['main', 'friends', 'messages', 'admin'];
  private router = inject(Router);
  private accountService = inject(AccountService);

  goToEditProfile(): void {
    this.router.navigate(['member/' + this.accountService.loggedInUserSig()?.userName], {skipLocationChange: true});
  }

  logout() {
    this.accountService.logout();
  }
}
