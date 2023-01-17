import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  constructor(public accountService: AccountService) { }

  user$!: Observable<User | null>;

  links = ['members', 'lists', 'messages'];
  activeLink = this.links[0];

  ngOnInit(): void {
    this.user$ = this.accountService.currentUser$;
  }

  // prevent changing tab if user refreshes the page. 
  removeReturnUrl() {
    localStorage.removeItem('returnUrl');
  }

  logout() {
    this.accountService.logout();
  }
}
