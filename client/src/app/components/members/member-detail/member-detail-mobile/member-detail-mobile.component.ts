import {AfterViewChecked, Component, effect, inject, OnInit, ViewChild} from '@angular/core';
import {MatSnackBar} from '@angular/material/snack-bar';
import {ActivatedRoute, Params, Router, RouterModule} from '@angular/router';
import {Gallery, GalleryItem, GalleryModule, ImageItem} from 'ng-gallery';
import {take} from 'rxjs';
import {ApiResponseMessage} from '../../../../models/helpers/api-response-message';
import {Member} from '../../../../models/member.model';
import {AccountService} from '../../../../services/account.service';
import {FollowService} from '../../../../services/follow.service';
import {MemberService} from '../../../../services/member.service';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatTabGroup, MatTabsModule} from '@angular/material/tabs';
import {IntlModule} from 'angular-ecmascript-intl';
import {LightboxModule} from 'ng-gallery/lightbox';
import {ShortenStringPipe} from '../../../../pipes/shorten-string.pipe';
import {MatDividerModule} from '@angular/material/divider';
import {MatIconModule} from '@angular/material/icon';
import {MemberMessagesComponent} from '../../member-messages/member-messages.component';
import {PresenceService} from '../../../../services/hubs/presence.service';
import {MessageService} from '../../../../services/message.service';
import {CommonService} from "../../../../services/common.service";

@Component({
  selector: 'app-member-detail-mobile',
  imports: [
    CommonModule, NgOptimizedImage, RouterModule, ShortenStringPipe,
    MemberMessagesComponent,
    MatCardModule, MatTabsModule, MatButtonModule, MatDividerModule, MatIconModule,
    GalleryModule, LightboxModule, IntlModule
  ],
  templateUrl: './member-detail-mobile.component.html',
  styleUrl: './member-detail-mobile.component.scss'
})
export class MemberDetailMobileComponent implements OnInit, AfterViewChecked {
  @ViewChild('tabGroup') tabGroup: MatTabGroup | undefined;
  @ViewChild(MemberMessagesComponent) memberMessages: MemberMessagesComponent | undefined;
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  router = inject(Router);
  onlineUsersSig = inject(PresenceService).onlineUsersSig;
  initLoad = true;
  isChatActive = false;
  readonly messagesTabIndex = 2;
  member: Member | undefined;
  images: GalleryItem[] = [];
  readonly imageWH = 50;
  private _memberService = inject(MemberService);
  private _messageService = inject(MessageService);
  private _followService = inject(FollowService);
  private _isMemberMessageCompSig = inject(CommonService).isMemberMessageCompSig;
  private _route = inject(ActivatedRoute);
  private _snackBar = inject(MatSnackBar);
  private _gallery = inject(Gallery);

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
    const userName = this.getUserNameFromParam();
    console.log(userName);
    if (userName) {
      this._memberService.getMemberByUsername(userName)
        ?.pipe(take(1))
        .subscribe((member: Member) => {
          if (member)
            this.member = member;
        });
    }
  }

  addFollow(member: Member): void {
    this._followService.addFollow(member.userName)
      .pipe(
        take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          this._snackBar.open(response.message, "Close", {
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
          this._snackBar.open(response.message, "Close", {
            horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000
          });

          member.isFollowing = false;
        }
      });
  }

  setGalleryImages(): void {
    this.images = []; // reset images for new member$ before loading in galleryRef

    if (this.member)
      for (const photo of this.member.photos) {
        this.images.push(new ImageItem({src: photo.url_enlarged, thumb: photo.url_165}));
      }

    // load ng-gallery and insert images
    const galleryRef = this._gallery.ref();
    galleryRef.load(this.images);
  }

  // TODO Add counter badge to show the unread messages on the messages tab icon
  setTabGroupParam(): void {
    this._route.queryParams.pipe(
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

      if (tabIndex === this.messagesTabIndex) {
        await this.createMessageHubConnection();
        this._isMemberMessageCompSig.set(true);
        this.isChatActive = true;
        this.memberMessages?.getMessages();
        this.memberMessages?.initBufferSizeAndViewport();
        this._messageService.scrollToBottom();
      } else {
        this._isMemberMessageCompSig.set(false);
        await this._messageService.stopHubConnection();
        this.isChatActive = false;
      }
    }
  }

  async createMessageHubConnection(): Promise<void> {
    const token = this.loggedInUserSig()?.token;
    if (token) {
      await this._messageService.createHubConnection(token);
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

  /**
   * Basic way: snapshot takes current URL only once. Angular doesn't detect if another userName(URL) is entered to refresh the MemberDetails for the new URL.
   * const userName: string | null = this.route.snapshot.paramMap.get('userName');
   *
   * Advance way: params.pipe makes Angular detect if the URL is changed to a new userName(URL change) and gets the new Member to update the DOM
   * @returns userName: string | undefined
   */
  private getUserNameFromParam(): string | undefined {
    let userName;

    this._route.params.pipe(take(1))
      .subscribe((params: Params) => {
        if ((params['userName']))
          userName = params['userName'];
      });

    return userName ? userName : undefined;
  }
}
