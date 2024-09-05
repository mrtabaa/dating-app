import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, Input, effect, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { Member } from '../../../../models/member.model';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { Observable, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { PresenceService } from '../../../../services/hubs/presence.service';

@Component({
  selector: 'app-member-card-mobile',
  standalone: true,
  imports: [
    CommonModule, RouterLink, NgOptimizedImage,
    ShortenStringPipe,
    MatCardModule, MatIconModule
  ],
  templateUrl: './member-card-mobile.component.html',
  styleUrl: './member-card-mobile.component.scss'
})
export class MemberCardMobileComponent {
  @Input() memberIn: Member | undefined;
  private breakpointObserver = inject(BreakpointObserver);
  isSmallPhone$: Observable<boolean> | undefined;
  isLargePhone$: Observable<boolean> | undefined;
  isTablet$: Observable<boolean> | undefined;
  onlineUsersSig = inject(PresenceService).onlineUsersSig;

  constructor() {
    this.setScreenSize();

    this.updateOnlineUser();
  }

  private updateOnlineUser(): void {
    effect(() => {
      const onlineUser = this.onlineUsersSig().find(x => x.userName === this.memberIn?.userName.toUpperCase());

      if (this.memberIn) {
        if (onlineUser) {
          this.memberIn.isOnline = true;
          this.memberIn.lastActive = onlineUser.lastActive;
        }
        else {
          this.memberIn.isOnline = false;
        }
      }
    });
  }

  private setScreenSize(): void {
    this.isSmallPhone$ = this.breakpointObserver.observe('(min-width: 350px)')
      .pipe(map(({ matches }) => {
        matches = matches ? false : true

        return matches;
      }));

    this.isLargePhone$ = this.breakpointObserver.observe('(min-width: 430rem)')
      .pipe(map(({ matches }) => {
        matches = matches ? false : true

        return matches;
      }));

    this.isTablet$ = this.breakpointObserver.observe('(min-width: 600rem)')
      .pipe(map(({ matches }) => {
        matches = matches ? false : true

        return matches;
      }));
  }
}
