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

  constructor(private authService: AccountService) { }

  user$!: Observable<User | null>;
  links = ['members', 'lists', 'messages'];
  activeLink = this.links[0];

  ngOnInit(): void {
    this.getCurrentUser();
  }

  getCurrentUser() {
    this.user$ = this.authService.currentUser$;
  }

  logout() {
    this.authService.logout();
  }
}
