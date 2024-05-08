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
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';

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
        next: (response: ApiResponseMessage) => 
          this.snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          })
      });
    }
  }
}
