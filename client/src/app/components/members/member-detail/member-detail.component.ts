import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
// import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { Observable, Subscription } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Member } from '../../../models/member.model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MemberService } from '../../../services/member.service';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage,
    MatCardModule, MatTabsModule
  ],
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  private memberService = inject(MemberService);
  private route = inject(ActivatedRoute);

  member$: Observable<Member> | undefined;
  apiPhotoUrl = environment.apiPhotoUrl;
  subscribed: Subscription | undefined;

  // galleryOptions: NgxGalleryOptions[] = [];
  // galleryImages: NgxGalleryImage[] = [];

  ngOnInit(): void {
    this.loadUser();
    this.setGalleryImages();
    // this.setGalleryOptions();
  }

  ngOnDestroy(): void {
    this.subscribed;
  }

  loadUser(): void {
    const email: string | null = this.route.snapshot.paramMap.get('email');

    if (email) {
      this.member$ = this.memberService.getMember(email);
    }
  }

  setGalleryImages(): void {
    this.subscribed = this.member$?.subscribe(
      (user: Member) => {
        // for (const photo of user.photos) {
        //   this.galleryImages.push(
        //     {
        //       small: this.apiPhotoUrl + photo.url_128,
        //       medium: this.apiPhotoUrl + photo.url_512,
        //       big: this.apiPhotoUrl + photo.url_1024
        //     }
        //   );
        // }
      }
    );
  }

  // setGalleryOptions(): void {
  //   this.galleryOptions = [
  //     {
  //       width: '20rem',
  //       height: '20rem',
  //       imagePercent: 100,
  //       thumbnailsColumns: 4,
  //       imageAnimation: NgxGalleryAnimation.Slide,
  //       preview: false
  //     }
  //   ];
  // }
}
