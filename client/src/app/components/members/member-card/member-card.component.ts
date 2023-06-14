import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/models/member.model';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.scss']
})
export class MemberCardComponent implements OnInit {
  @Input('memberInput') member: Member | undefined;

  mainUrl: string | undefined;

  ngOnInit(): void {
    this.setMainUrl();
  }
  
  setMainUrl(): void {
    if (this.member)
    for (const photo of this.member.photos) {
      if (photo.isMain) {
        this.mainUrl = photo.url_128;
      }
    }
  }
}
