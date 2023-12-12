import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { User } from '../../../models/user.model';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';


@Component({
  selector: 'app-user-card',
  standalone: true,
  imports: [
    CommonModule,
    NgOptimizedImage, RouterLink,
    MatCardModule, MatIconModule
  ],
  templateUrl: './user-card.component.html',
  styleUrls: ['./user-card.component.scss']
})
export class UserCardComponent {
  @Input('userInput') user: User | undefined;

  apiPhotoUrl = environment.apiPhotoUrl;
}
