import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
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
export class PhotoEditorComponent implements OnInit {
  @Input('member') member: Member | undefined;
  user!: User | null;
  errorGlob: string | undefined;

  basePhotoUrl: string = environment.apiPhotoUrl;

  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;

  constructor(private accountService: AccountService, private memberService: MemberService) {
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user)
          this.user = user
      }
    })
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader(): void {
    this.uploader = new FileUploader({
      url: environment.apiUrl + 'user/add-photos',
      authToken: 'Bearer ' + this.user?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 40_000_000, // 8 * 5MB
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        this.member?.photos.push(photo);
      }
    }
  }

  setMainPhoto(url_128In: string): void {
    this.memberService.setMainPhoto(url_128In).pipe(take(1)).subscribe({
      next: updateResult => {
        if (updateResult.modifiedCount === 1 && this.member && this.user) {
          this.member.photos.forEach(photo => {
            // unset previous main
            if (photo.isMain === true)
              photo.isMain = false;

            // set new selected
            if (this.user && photo.url_128 === url_128In) {
              photo.isMain = true;

              // update navbar photos
              this.user.profilePhotoUrl = url_128In;
              this.accountService.setCurrentUser(this.user);
            }
          })
        }

        this.errorGlob = undefined;
      },
      error: err => this.errorGlob = err.message // if set fails
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
