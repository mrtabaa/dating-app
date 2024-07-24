import { AfterViewChecked, Component, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
import { Observable, Subscription, take } from 'rxjs';
import { Member } from '../../../models/member.model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { MemberService } from '../../../services/member.service';
import { MatButtonModule } from '@angular/material/button';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { LightboxModule } from 'ng-gallery/lightbox';
import { IntlModule } from "angular-ecmascript-intl";
import { AccountService } from '../../../services/account.service';
import { FollowService } from '../../../services/follow.service';
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { ResponsiveService } from '../../../services/responsive.service';
import { MemberDetailMobileComponent } from './member-detail-mobile/member-detail-mobile.component';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, RouterModule,
    MatCardModule, MatTabsModule, MatButtonModule, MatIconModule,
    MemberDetailMobileComponent, MemberMessagesComponent,
    MatCardModule, MatTabsModule, MatButtonModule,
    GalleryModule, LightboxModule, IntlModule
  ],
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss']
})
export class MemberDetailComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('tabGroup') tabGroup: MatTabGroup | undefined;
  @ViewChild('memberMessage') memberMessage: MemberMessagesComponent | undefined;

  private memberService = inject(MemberService);
  private followService = inject(FollowService);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  username = inject(AccountService).loggedInUserSig()?.userName;
  private snackBar = inject(MatSnackBar);
  private gallery = inject(Gallery);
  router = inject(Router);
  private route = inject(ActivatedRoute);
  initLoad = true;
  readonly messageTabIndex = 3;

  member$: Observable<Member> | undefined;
  subscribed: Subscription | undefined;
  images: GalleryItem[] = [];

  ngOnInit(): void {
    this.getMember();
    this.setGalleryImages();
  }

  ngAfterViewChecked(): void {
    if (this.initLoad && this.tabGroup) {
      this.setTabGroupParam(); // ViewChild is read in this lifeCycle only since it's in @if(async)

      this.initLoad = false;
    }
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

  setTabGroupParam(): void {
    this.route.queryParams.pipe(
      take(1)).subscribe(params => {
        const tab = params['tab'];
        if (tab)
          this.setSelectTabIndex(tab);
      });
  }

  setSelectTabIndex(tabIndex: number): void {
    if (this.tabGroup) {
      this.tabGroup.selectedIndex = tabIndex;
      this.router.navigate([], { queryParams: { tab: tabIndex }, queryParamsHandling: 'merge' });

      this.memberMessage?.initBufferSize();
      this.memberMessage?.scrollToBottom();
    }
  }
}