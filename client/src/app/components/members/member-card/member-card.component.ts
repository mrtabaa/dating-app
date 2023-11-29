import { Component, Input } from '@angular/core';
import { user } from 'src/app/models/user.model';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-user-card',
  templateUrl: './user-card.component.html',
  styleUrls: ['./user-card.component.scss']
})
export class MemberCardComponent {
  @Input('memberInput') user: user | undefined;

  apiPhotoUrl = environment.apiPhotoUrl;
}
