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
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { LoggedInUser } from '../../../models/logged-in-user.model';
import { Photo } from '../../../models/photo.model';
import { MatSnackBar } from '@angular/material/snack-bar';

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
  private snackBar = inject(MatSnackBar);

  loggedInUser: LoggedInUser | null | undefined;
  baseApiUrl: string = environment.apiUrl;
  basePhotoUrl: string = environment.apiPhotoUrl;

  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;

  constructor() {
    this.loggedInUser = this.accountService.loggedInUserSig();
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  //#region Photo Upload using `ng2-file-upload`
  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(): void {
    if (this.loggedInUser) {
      this.uploader = new FileUploader({
        url: this.baseApiUrl + 'user/add-photo',
        authToken: 'Bearer ' + this.loggedInUser.token,
        isHTML5: true,
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 4_000_000, // 4MB
      });

      this.uploader.onAfterAddingFile = (file) => {
        file.withCredentials = false;
      }

      this.uploader.onSuccessItem = (item, response, status, headers) => {
        if (response) {
          const photo: Photo = JSON.parse(response);
          this.member?.photos.push(photo);

          // set navbar profile photo when first photo is uploaded
          if (this.member?.photos.length === 1)
            this.setNavbarProfilePhoto(photo.url_165)
        }
      }
    }
  }
  //#endregion Photo Upload using `ng2-file-upload`

  /**
   * Set navbar profile photo ONLY when FIRST photo is uploaded.
   * @param url_165 
   */
  setNavbarProfilePhoto(url_165: string): void {
    if (this.loggedInUser) {

      this.loggedInUser.profilePhotoUrl = url_165;

      this.accountService.loggedInUserSig.set(this.loggedInUser)
    }
  }

  /**
   * Set main photo for card and album
   * @param url_165In 
   */
  setMainPhoto(url_165In: string): void {
    this.userService.setMainPhoto(url_165In).pipe(take(1)).subscribe({
      next: (response: string) => {
        if (response && this.member) {

          this.member.photos.forEach(photo => {
            // unset previous main
            if (photo.isMain === true)
              photo.isMain = false;

            // set new selected main
            if (photo.url_165 === url_165In) {
              photo.isMain = true;

              // update navbar photo
              this.loggedInUser!.profilePhotoUrl = url_165In;
              this.accountService.setCurrentUser(this.loggedInUser!);
            }
          })

          this.snackBar.open(response, 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000 });
        }
      }
    });
  }

  deletePhoto(url_128In: string, index: number): void {
    this.userService.deletePhoto(url_128In).pipe(take(1)).subscribe({
      next: (response: string) => {
        if (response && this.member) {
          this.member.photos.splice(index, 1);

          this.snackBar.open(response, 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000 });
        }
      }
    })
  }
}
