import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTabsModule, MatTabChangeEvent } from '@angular/material/tabs';
import { Observable, tap } from 'rxjs';
import { Member } from '../../models/member.model';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { FollowService } from '../../services/follow.service';

@Component({
  selector: 'app-follows',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MemberCardComponent,
    MatTabsModule
  ],
  templateUrl: './follows.component.html',
  styleUrl: './follows.component.scss'
})
export class FollowsComponent implements OnInit {
  private followService = inject(FollowService);

  members$: Observable<Member[]> | undefined;
  predicate: string = 'followings';
  isMembersValid: boolean = true;

  ngOnInit(): void {
    this.getFollows();
  }

  getFollows(): void {
    this.isMembersValid = true; // reset to default.
    
    this.members$ = this.followService.getLikes(this.predicate).pipe(
      tap(member => {
        this.isMembersValid = member ? true : false;
      })
    );
  }

  /**
   * Set predicate based on each tab then call getFollows()
   * @param event 
   */
  onTabChange(event: MatTabChangeEvent) { // called on tab change
    if (event.tab.textLabel == "Following") {
      this.predicate = 'followings';
      this.getFollows();
    }
    else {
      this.predicate = 'followers';
      this.getFollows()
    }
  }
}
