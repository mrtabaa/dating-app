import { Component, OnInit, inject } from '@angular/core';
import { UserService } from '../../services/user.service';
import { Observable } from 'rxjs';
import { CommonModule, NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit {
  private userService = inject(UserService);
  photos$: Observable<string[]> | undefined;

  ngOnInit(): void {
    this.getMemberPhotos();
  }

  getMemberPhotos(): void {
    this.photos$ = this.userService.getAllPhotos();
  }
}
