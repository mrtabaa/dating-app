import { Component, Input } from '@angular/core';
import { Member } from 'src/app/models/member.model';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.scss']
})
export class MemberCardComponent {
  @Input('memberInput') member: Member | undefined;

  apiPhotoUrl = environment.apiPhotoUrl;
}
