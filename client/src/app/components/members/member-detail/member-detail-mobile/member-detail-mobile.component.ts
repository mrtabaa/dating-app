import { Component, OnInit, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Params, Router, RouterModule } from '@angular/router';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { Observable, Subscription, of, switchMap, take } from 'rxjs';
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
export class MemberDetailMobileComponent implements OnInit {
  private memberService = inject(MemberService);
  private followService = inject(FollowService);
  username = inject(AccountService).loggedInUserSig()?.userName;
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);
  private gallery = inject(Gallery);
  router = inject(Router);

  member$: Observable<Member | null> | undefined;
  subscribed: Subscription | undefined;
  images: GalleryItem[] = [];
  readonly imageWH = 50;

  ngOnInit(): void {
    this.getMember();
  }

  getMember(): void {
    // Basic way: snapshot takes current URL only once. Angular doesn't detect if another userName(URL) is entered to refresh the MemberDetails for the new URL.
    // const userName: string | null = this.route.snapshot.paramMap.get('userName'); 

    // Advance way: params.pipe makes Angular detect if the URL is changed to a new userName(URL change) and gets the new Member to update the DOM
    this.member$ = this.route.params.pipe(
      switchMap((params: Params): Observable<Member | null> => {
        const userName = params['userName'];
        if (!userName) {
          // Return an Observable that emits null
          return of(null);
        }
        // Call the service and ensure that an Observable is always returned
        const memberObservable = this.memberService.getMemberByUsername(userName);
        return memberObservable ? memberObservable : of(null);
      })
    );

    if (this.member$ !== of(null))
      this.setGalleryImages();
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
    this.subscribed = this.member$?.subscribe((member: Member | null) => {
      this.images = []; // reset images for new member$ before loading in galleryRef
      
      if (member)
        for (const photo of member.photos) {
          this.images.push(new ImageItem({ src: photo.url_enlarged, thumb: photo.url_165 }));
        }

      // load ng-gallery and insert images
      const galleryRef = this.gallery.ref();
      galleryRef.load(this.images)
    });
  }
}
