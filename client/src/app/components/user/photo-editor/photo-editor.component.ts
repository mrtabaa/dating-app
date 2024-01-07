import { Component, Input, OnInit, inject } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { FileUploadModule, FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { Member } from '../../../models/member.model';
import { environment } from '../../../../environments/environment';
import { AccountService } from '../../../services/account.service';
import { UserService } from '../../../services/user.service';
import { UpdateResult } from '../../../models/helpers/update-result.model';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { Photo } from '../../../models/photo.model';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage,
    MatIconModule, MatFormFieldModule, MatCardModule, MatButtonModule,
    FileUploadModule
  ],
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent implements OnInit {
  @Input('member') member: Member | undefined;
  private accountService = inject(AccountService);
  private userService = inject(UserService);

  loggedInUser: LoggedInUser | null | undefined;
  errorGlob: string | undefined;
  basePhotoUrl: string = environment.apiPhotoUrl;

  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;

  constructor() {
    this.loggedInUser = this.accountService.loggedInUserSig();
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(): void {
    if (this.loggedInUser) {
      this.uploader = new FileUploader({
        url: environment.apiUrl + 'user/add-photo',
        authToken: 'Bearer ' + this.loggedInUser.token,
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
          const photo: Photo = JSON.parse(response);
          this.member?.photos.push(photo);

          if(this.member?.photos.length === 0) 
            // this.accountService.loggedInUserSig.update(user => user?.profilePhotoUrl, photo.url_165) // TODO update navbar when 1st photo is added
        }
      }
    }
  }

  setMainPhoto(url_165In: string): void {
    this.userService.setMainPhoto(url_165In).pipe(take(1)).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.modifiedCount === 1 && this.loggedInUser && this.member) {
          this.member.photos.forEach(photo => {
            // unset previous main
            if (photo.isMain === true)
              photo.isMain = false;

            // set new selected main
            if (photo.url_165 === url_165In) {
              photo.isMain = true;

              // update navbar photos
              this.loggedInUser!.profilePhotoUrl = url_165In;
              this.accountService.setCurrentUser(this.loggedInUser!);
            }
          })
        }

        this.errorGlob = undefined;
      },
      error: err => this.errorGlob = err.message // if set fails
    });
  }

  deletePhoto(url_128In: string, index: number): void {
    this.userService.deletePhoto(url_128In).pipe(take(1)).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.modifiedCount === 1 && this.member) {
          this.member.photos.splice(index, 1);
        }
      }
    })
  }
}
