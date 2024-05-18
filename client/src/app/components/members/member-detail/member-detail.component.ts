import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
import { Observable, Subscription } from 'rxjs';
import { Member } from '../../../models/member.model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MemberService } from '../../../services/member.service';
import { MatButtonModule } from '@angular/material/button';
import { Gallery, GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { LightboxModule } from 'ng-gallery/lightbox';
import { IntlModule } from "angular-ecmascript-intl";
import { AccountService } from '../../../services/account.service';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage, RouterModule,
    MatCardModule, MatTabsModule, MatButtonModule,
    GalleryModule, LightboxModule, IntlModule
  ],
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  private memberService = inject(MemberService);
  username = inject(AccountService).loggedInUserSig()?.userName;
  private route = inject(ActivatedRoute);
  private gallery = inject(Gallery);
  router = inject(Router);

  member$: Observable<Member> | undefined;
  subscribed: Subscription | undefined;
  images: GalleryItem[] = [];

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
