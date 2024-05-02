import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Member } from '../../../models/member.model';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../pipes/shorten-string.pipe';
import { take } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FollowService } from '../../../services/follow.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [
    CommonModule,
    NgOptimizedImage, RouterLink, ShortenStringPipe,
    MatCardModule, MatIconModule
  ],
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.scss']
})
export class MemberCardComponent {
  @Input() member: Member | undefined;

  private followService = inject(FollowService);
  private snackBar = inject(MatSnackBar);

  addFollow(): void {
    if (this.member?.userName) {
      this.followService.addFollow(this.member.userName).pipe(take(1)).subscribe({
        next: () =>
          this.snackBar.open("You've followed " + this.member?.knownAs + '.', "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          })
        ,
        error: err => this.snackBar.open(err.error, "Close", {
          horizontalPosition: 'center', verticalPosition: 'top', duration: 7000
        })
      });
    }
  }
}
