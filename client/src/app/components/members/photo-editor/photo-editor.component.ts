import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { Observable, map } from 'rxjs';
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
  @Input('member') member$: Observable<Member> | undefined;
  photoUrls: PhotoUrl[] = [];

  constructor(private memberService: MemberService, private cd: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.getPhotoUrls();
  }

  getPhotoUrls(): void {
    this.member$?.pipe(
      map((member: Member) => {
        for (const photo of member.photos) {
          this.photoUrls.push(
            {
              url_128: environment.apiPhotoUrl + photo.url_128,
              isMain: photo.isMain
            });
        }
      }))
  }

  setMainPhoto(idIn: string | undefined, url_128In: string): void {
    this.memberService.setMainPhoto(idIn, url_128In);
  }
}
