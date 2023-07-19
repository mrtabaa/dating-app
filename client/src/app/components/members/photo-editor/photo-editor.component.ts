import { Component, Inject, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/models/member.model';
import { MemberService } from 'src/app/services/member.service';
import { environment } from 'src/environments/environment';

interface PhotoUrl {
  url_128: string,
  isMain: boolean
}

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent implements OnInit {
  @Input('member') member: Member | undefined;
  private memberService = Inject(MemberService);
  photoUrls: PhotoUrl[] = [];

  ngOnInit(): void {
    this.getPhotoUrls();
  }

  getPhotoUrls(): void {
    if (this.member)
      for (const photo of this.member.photos) {
        this.photoUrls.push(
          {
            url_128: environment.apiPhotoUrl + photo.url_128,
            isMain: photo.isMain
          });
      }
  }

  // setMainPhoto(): void {

  // }
}
