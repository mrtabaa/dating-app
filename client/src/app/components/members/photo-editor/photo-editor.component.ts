import { Component, Input } from '@angular/core';
import { take } from 'rxjs';
import { Member } from 'src/app/models/member.model';
import { User } from 'src/app/models/user.model';
import { AccountService } from 'src/app/services/account.service';
import { MemberService } from 'src/app/services/member.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent {
  @Input('member') member: Member | undefined;
  user!: User | null;
  errorGlob: string | undefined;

  basePhotoUrl: string = environment.apiPhotoUrl;

  constructor(private accountService: AccountService, private memberService: MemberService) {
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
  }

  setMainPhoto(url_128In: string): void {
    this.memberService.setMainPhoto(url_128In).pipe(take(1)).subscribe({
      next: updateResult => {
        if (updateResult.modifiedCount === 1 && this.member && this.user) {
          this.member.photos.forEach(photo => {
            if (photo.isMain === true)
              photo.isMain = false;

            if (this.user && photo.url_128 === url_128In) {
              photo.isMain = true;
              this.user.profilePhotoUrl = url_128In;
              this.accountService.setCurrentUser(this.user);
            }
          })
        }

        this.errorGlob = undefined;
      },
      error: err => this.errorGlob = err.message
    });
  }

  deletePhoto(url_128In: string, index: number): void {
    this.memberService.deletePhoto(url_128In).pipe(take(1)).subscribe({
      next: updateResult => {
        if (updateResult.modifiedCount === 1 && this.member) {
          this.member.photos.splice(index, 1);
        }
      }
    })
  }
}
