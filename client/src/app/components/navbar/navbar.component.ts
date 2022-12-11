import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { UserProfile } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  constructor(private accountService: AccountService) { }

  user$!: Observable<UserProfile | null>;

  ngOnInit(): void {
    this.getCurrentUser();
  }

  getCurrentUser() {
    this.user$ = this.accountService.currentUser$;
  }

  logout() {
    this.accountService.logout();
  }
}
