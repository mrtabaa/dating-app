import { Component, Input, inject } from '@angular/core';
import { Member } from 'src/app/models/member.model';
import { MemberService } from 'src/app/services/member.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent {
  @Input('member') member: Member | undefined;

}
