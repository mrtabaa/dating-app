import { Component, Input, OnInit, inject } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
// import { FileUploadModule } from 'ng2-file-upload';
// import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { LoggedInUser } from '../../../models/loggedInUser.model';
import { User } from '../../../models/user.model';
import { environment } from '../../../../environments/environment';
import { AccountService } from '../../../services/account.service';
import { UserService } from '../../../services/user.service';
import { UpdateResult } from '../../../models/helpers/update-result.model';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [
    NgOptimizedImage,
    MatIconModule, MatFormFieldModule
  ],
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.scss']
})
export class PhotoEditorComponent implements OnInit {
  @Input('user') user: User | undefined;
  private accountService = inject(AccountService);
  private userService = inject(UserService);

  loggedInUser: LoggedInUser | undefined;
  errorGlob: string | undefined;
  basePhotoUrl: string = environment.apiPhotoUrl;

  // uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;

  constructor() {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (loggedInUser: LoggedInUser | null) => {
        if (loggedInUser)
          this.loggedInUser = loggedInUser
      }
    });
  }

  ngOnInit(): void {
    // this.initializeUploader();
  }

  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  // initializeUploader(): void {
  //   this.uploader = new FileUploader({
  //     url: environment.apiUrl + 'user/add-photos',
  //     authToken: 'Bearer ' + this.user?.token,
  //     isHTML5: true,
  //     allowedFileType: ['image'],
  //     removeAfterUpload: true,
  //     autoUpload: false,
  //     maxFileSize: 40_000_000, // 8 * 5MB
  //   });

  //   this.uploader.onAfterAddingFile = (file) => {
  //     file.withCredentials = false;
  //   }

  //   this.uploader.onSuccessItem = (item, response, status, headers) => {
  //     if (response) {
  //       const photo = JSON.parse(response);
  //       this.user?.photos.push(photo);
  //     }
  //   }
  // }

  setMainPhoto(url_128In: string): void {
    this.userService.setMainPhoto(url_128In).pipe(take(1)).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.modifiedCount === 1 && this.loggedInUser && this.user) {
          this.user.photos.forEach(photo => {
            // unset previous main
            if (photo.isMain === true)
              photo.isMain = false;

            // set new selected main
            if (this.loggedInUser && photo.url_128 === url_128In) {
              photo.isMain = true;

              // update navbar photos
              this.loggedInUser.profilePhotoUrl = url_128In;
              this.accountService.setCurrentUser(this.loggedInUser);
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
        if (updateResult.modifiedCount === 1 && this.user) {
          this.user.photos.splice(index, 1);
        }
      }
    })
  }
}
