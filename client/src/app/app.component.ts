import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './services/account.service';
import { NgxSpinnerModule } from "ngx-spinner";
import { NavbarComponent } from './components/navbar/navbar.component';
import { UserService } from './services/user.service';
import { take } from 'rxjs';
import { LoggedInUser } from './models/logged-in-user.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, NgxSpinnerModule,
    NavbarComponent
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
    this.getLocalStorageCurrentValues();
  }

  getLocalStorageCurrentValues() {
    const token = localStorage.getItem('token');

    if (token) {
      this.accountService.getLoggedInUser().pipe(take(1)).subscribe(
        {
          next: (loggedInUser: LoggedInUser | null) => {
            if (loggedInUser)
              this.accountService.setCurrentUser(loggedInUser);
          },
          error: (err) => { //if token is expired and api call is unauthorized.
            console.log(err.error);
            this.accountService.logout()
          } 
        });
    }
  }
}
