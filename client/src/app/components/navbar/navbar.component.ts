import { Component, OnInit } from '@angular/core';
import { Observable, map } from 'rxjs';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  constructor(public accountService: AccountService, private memberService: MemberService) { }

  user$!: Observable<User | null>;

  links = ['members', 'lists', 'messages'];

  ngOnInit(): void {
    this.user$ = this.accountService.currentUser$;
    

  }

  getMainPhotoUrl(): string {
    this.user$.pipe(
      map(user => {
      })
    )
  }

  logout() {
    this.accountService.logout();
  }
}
