import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { Observable, Subscription, take } from 'rxjs';
import { ApiResponseMessage } from '../../../../models/helpers/api-response-message';
import { Member } from '../../../../models/member.model';
import { AccountService } from '../../../../services/account.service';
import { FollowService } from '../../../../services/follow.service';
import { MemberService } from '../../../../services/member.service';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { IntlModule } from 'angular-ecmascript-intl';
import { LightboxModule } from 'ng-gallery/lightbox';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-member-detail-mobile',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, RouterModule, ShortenStringPipe,
    MatCardModule, MatTabsModule, MatButtonModule, MatDividerModule, MatIconModule,
    GalleryModule, LightboxModule, IntlModule
  ],
  templateUrl: './member-detail-mobile.component.html',
  styleUrl: './member-detail-mobile.component.scss'
})
export class MemberDetailMobileComponent implements OnInit, OnDestroy {
  private memberService = inject(MemberService);
  private followService = inject(FollowService);
  username = inject(AccountService).loggedInUserSig()?.userName;
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);
  private gallery = inject(Gallery);
  router = inject(Router);

  member$: Observable<Member> | undefined;
  subscribed: Subscription | undefined;
  images: GalleryItem[] = [];
  readonly imageWH = 70;

  ngOnInit(): void {
    this.getMember();
    this.setGalleryImages();
  }

  ngOnDestroy(): void {
    this.subscribed;
  }

  getMember(): void {
    const userName: string | null = this.route.snapshot.paramMap.get('userName');

    if (userName) {
      this.member$ = this.memberService.getMemberByUsername(userName);
    }
  }

  addFollow(member: Member): void {
    this.followService.addFollow(member.userName)
      .pipe(
        take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          this.snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          });

          member.isFollowing = true;
        }
      });
  }

  removeFollow(member: Member): void {
    this.followService.removeFollow(member.userName)
      .pipe(
        take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          this.snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          });

          member.isFollowing = false;
        }
      });
  }

  setGalleryImages(): void {
    this.subscribed = this.member$?.subscribe(
      (member: Member) => {
        for (const photo of member.photos) {
          this.images.push(new ImageItem({ src: photo.url_enlarged, thumb: photo.url_165 }));
        }

        // load ng-gallery and insert images
        const galleryRef = this.gallery.ref();
        galleryRef.load(this.images)
      }
    );
  }
}
