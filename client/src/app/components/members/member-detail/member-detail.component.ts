import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
import { Observable, Subscription } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Member } from '../../../models/member.model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MemberService } from '../../../services/member.service';
import { MatButtonModule } from '@angular/material/button';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage,
    MatCardModule, MatTabsModule, MatButtonModule,
    GalleryModule
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
  images: GalleryItem[] = [];

  ngOnInit(): void {
    this.getMember();
    this.setGalleryImages();
  }

  ngOnDestroy(): void {
    this.subscribed;
  }

  getMember(): void {
    const id: string | null = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.member$ = this.memberService.getMember(id);
    }
  }

  setGalleryImages(): void {
    this.subscribed = this.member$?.subscribe(
      (member: Member) => {
        for (const photo of member.photos) {
          this.images.push(new ImageItem({src: this.apiPhotoUrl + photo.url_1024, thumb: this.apiPhotoUrl + photo.url_128}));
        }
      }
    );
  }
}
