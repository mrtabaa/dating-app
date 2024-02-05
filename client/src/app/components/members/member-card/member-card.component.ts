import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { Member } from '../../../models/member.model';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ShortenStringPipe } from '../../../pipes/shorten-string.pipe';
import { LikeService } from '../../../services/like.service';
import { take } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

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
  @Input('memberInput') member: Member | undefined;

  private likeService = inject(LikeService);
  private snackBar = inject(MatSnackBar);

  apiPhotoUrl = environment.apiPhotoUrl;

  addLike(): void {
    if (this.member?.email) {
      this.likeService.addLike(this.member.email).pipe(take(1)).subscribe({
        next: () =>
          this.snackBar.open("You've liked " + this.member?.knownAs + '.', "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          })
        ,
        error: err => this.snackBar.open(err.error, "Close", {
          horizontalPosition: 'end', verticalPosition: 'bottom', duration: 7000
        })
      });
    }
  }
}
