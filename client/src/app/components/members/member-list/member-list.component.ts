import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/models/member.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit {
  members$: Observable<Member[]> | undefined;
  currentUser$: Observable<User | null> | undefined;

  constructor(private memberService: MemberService, accountService: AccountService) {
    this.currentUser$ = accountService.currentUser$;
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(): void {
    this.members$ = this.memberService.getMembers();
  }
}
