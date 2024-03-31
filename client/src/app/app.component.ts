import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './services/account.service';
import { NgxSpinnerModule } from "ngx-spinner";
import { NavbarComponent } from './components/navbar/navbar.component';
import { UserService } from './services/user.service';
import { FooterComponent } from './components/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, NgxSpinnerModule,
    NavbarComponent, FooterComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  userService = inject(UserService);

  title = 'Dating App';
  isLoading: boolean = false;

  ngOnInit(): void {
    this.initUserOnPageRefresh();
  }

  initUserOnPageRefresh() {
    const loggedInUserStr = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      this.accountService.setCurrentUser(JSON.parse(loggedInUserStr))

      // check if user's token is not expired.
      this.accountService.authorizeLoggedInUser();
    }
  }
}
