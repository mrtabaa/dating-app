import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { Member } from '../../../models/member.model';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';


@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [
    CommonModule,
    NgOptimizedImage, RouterLink,
    MatCardModule, MatIconModule
  ],
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.scss']
})
export class MemberCardComponent {
  @Input('memberInput') member: Member | undefined;

  apiPhotoUrl = environment.apiPhotoUrl;
}
