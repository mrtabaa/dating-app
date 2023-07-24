import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/models/member.model';
import { MemberService } from 'src/app/services/member.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent {
  @Input('member') member: Member | undefined;

  basePhotoUrl: string = environment.apiPhotoUrl;

  constructor(private memberService: MemberService) { }

  setMainPhoto(url_128In: string): void {
    this.member = this.memberService.setMainPhoto(url_128In);
  }
}
