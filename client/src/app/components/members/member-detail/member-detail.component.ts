import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { Observable, Subscription, map } from 'rxjs';
import { Member } from 'src/app/models/member.model';
import { MemberService } from 'src/app/services/member.service';

interface NgxItem {
  small: string,
  medium: string,
  big: string
}
@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  member$: Observable<Member> | undefined;
  mainUrl: string | undefined;
  subscribed: Subscription | undefined;

  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];

  constructor(private memberService: MemberService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadMember();
    this.galleryImages = this.loadImages();
    this.setMainUrl();
    this.setGalleryOptions();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  loadMember(): void {
    const email: string | null = this.route.snapshot.paramMap.get('email');

    if (email) {
      this.member$ = this.memberService.getMember(email);
    }
  }

  loadImages(): NgxItem[] {
    let imageUrls: NgxItem[] = [];

    this.subscribed = this.member$?.subscribe(
      ((member: Member) => {
        for (const photo of member.photos) {
          imageUrls.push(
            {
              small: photo.url,
              medium: photo.url,
              big: photo.url
            }
          );
        }
      })
    );
    
    return imageUrls;
  }

  setMainUrl(): void {
    if (this.member$) {
      this.subscribed = this.member$.subscribe(
        ((member: Member) => {
          for (const photo of member.photos) {
            if (photo.isMain) {
              this.mainUrl = photo.url;
            }
          }
        })
      );
    }
  }

  setGalleryOptions(): void {
    this.galleryOptions = [
      {
        width: '20rem',
        height: '20rem',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
  }

  // getImages(): any[] {
  //   let imageUrls: any[] = [];

  //   if (this.member$) {
  //     this.subscribed = this.member$.subscribe((member: Member) => {
  //       for (const photo of member.photos) {
  //         imageUrls.push(
  //           {
  //             small: photo.url,
  //             medium: photo.url,
  //             big: photo.url
  //           }
  //         );
  //       }
  //     });
  //   }

  //   return imageUrls;
  // }
}
