import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from './_models/user.model';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Dating App';

  constructor(private accountService: AccountService, private router: Router) { }

  ngOnInit(): void {
    this.setLocalStorageCurrentValues();
  }

  setLocalStorageCurrentValues() {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user: User = JSON.parse(userString);
      this.accountService.setCurrentUser(user);
    }

    const returnUrl = localStorage.getItem('returnUrl');
    if (returnUrl) {
      this.router.navigate([returnUrl]);
    }
  }
}
