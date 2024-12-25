import {AfterViewChecked, Component, effect, inject, OnInit, ViewChild} from '@angular/core';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {take} from 'rxjs';
import {Member} from '../../../models/member.model';
import {MatCardModule} from '@angular/material/card';
import {MatTabGroup, MatTabsModule} from '@angular/material/tabs';
import {MemberService} from '../../../services/member.service';
import {MatButtonModule} from '@angular/material/button';
import {Gallery, GalleryItem, GalleryModule, ImageItem} from 'ng-gallery';
import {LightboxModule} from 'ng-gallery/lightbox';
import {IntlModule} from "angular-ecmascript-intl";
import {AccountService} from '../../../services/account.service';
import {FollowService} from '../../../services/follow.service';
import {ApiResponseMessage} from '../../../models/helpers/api-response-message';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatIconModule} from '@angular/material/icon';
import {ResponsiveService} from '../../../services/responsive.service';
import {MemberDetailMobileComponent} from './member-detail-mobile/member-detail-mobile.component';
import {MemberMessagesComponent} from '../member-messages/member-messages.component';
import {PresenceService} from '../../../services/hubs/presence.service';
import {MessageService} from '../../../services/message.service';

@Component({
  selector: 'app-user-detail',
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
export class MemberDetailComponent implements OnInit, AfterViewChecked {
  @ViewChild('tabGroup') tabGroup: MatTabGroup | undefined;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  username = inject(AccountService).loggedInUserSig()?.userName;
  router = inject(Router);
  onlineUsersSig = inject(PresenceService).onlineUsersSig;
  initLoad = true;
  readonly messagesTabIndex = 3;
  isOnMessageTab = false;
  member: Member | undefined;
  images: GalleryItem[] = [];
  private _memberService = inject(MemberService);
  private _messageService = inject(MessageService);
  private _followService = inject(FollowService);
  private snackBar = inject(MatSnackBar);
  private gallery = inject(Gallery);
  private route = inject(ActivatedRoute);

  constructor() {
    this.updateOnlineUser();
  }

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

  getMember(): void {
    const userName: string | null = this.route.snapshot.paramMap.get('userName');

    if (userName) {
      this._memberService.getMemberByUsername(userName)
        ?.pipe(
          take(1)
        ).subscribe((member: Member) => {
        if (member) {
          this.member = member;
        }
      });
    }
  }

  addFollow(member: Member): void {
    this._followService.addFollow(member.userName)
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
    this._followService.removeFollow(member.userName)
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
    if (this.member)
      for (const photo of this.member.photos) {
        this.images.push(new ImageItem({src: photo.url_enlarged, thumb: photo.url_165}));
      }

    // load ng-gallery and insert images
    const galleryRef = this.gallery.ref();
    galleryRef.load(this.images)
  }

  setTabGroupParam(): void {
    this.route.queryParams.pipe(
      take(1)).subscribe(params => {
      const tab = params['tab'];
      if (tab)
        this.setSelectTabIndex(tab).finally();
    });
  }

  async setSelectTabIndex(tabIndex: number): Promise<void> {
    if (this.tabGroup) {
      this.tabGroup.selectedIndex = tabIndex;
      await this.router.navigate([], {queryParams: {tab: tabIndex}, queryParamsHandling: 'merge'});

      // Load/Destroy MessageComponent based on being on message tab or not
      this.isOnMessageTab = tabIndex == this.messagesTabIndex;
    }
  }

  private updateOnlineUser(): void {
    effect(() => {
      const onlineUser = this.onlineUsersSig().find(member => member.userName === this.member?.userName.toUpperCase());

      if (this.member) {
        if (onlineUser) {
          this.member.isOnline = true;
          this.member.lastActive = onlineUser.lastActive;
        } else {
          this.member.isOnline = false;
        }
      }
    });
  }
}
