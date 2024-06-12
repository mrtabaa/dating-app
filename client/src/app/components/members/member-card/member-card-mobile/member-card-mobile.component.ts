import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { Member } from '../../../../models/member.model';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { Observable, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';

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

  constructor() {
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
