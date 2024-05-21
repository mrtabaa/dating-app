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
import { ApiResponseMessage } from '../../../models/helpers/api-response-message';
import { PhotoDeleteResponse } from '../../../models/helpers/photo-delete-response';

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
  @Input() member: Member | undefined;
  private accountService = inject(AccountService);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);

  private readonly maxFileSize = 4 * 1024 * 1024; // 4MB in bytes
  private readonly minFileSize = 100_000; // 100KB in bytes

  loggedInUser: LoggedInUser | null | undefined;
  baseApiUrl: string = environment.apiUrl;

  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  isUploading = false;

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
        autoUpload: true,
        maxFileSize: this.maxFileSize,
      });

      this.uploader.onAfterAddingFile = (file) => {
        file.withCredentials = false;

        if (file.file.size < this.minFileSize) {
          this.snackBar.open('Photo has to be Larger than ' + Math.floor(this.minFileSize / 1000) + 'KB', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
          this.uploader?.clearQueue();
        }
      }

      this.uploader.onProgressAll = () => {
        this.isUploading = true;
      }

      this.uploader.onCancelItem = () => {
        this.isUploading = false;
      }

      this.uploader.onWhenAddingFileFailed = (file) => {
        if (file.size > this.maxFileSize)
          this.snackBar.open('Photo has to be Smaller than ' + Math.floor(this.maxFileSize / 1_000_000) + 'MB', 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
      }

      this.uploader.onSuccessItem = (item, response) => {
        if (response) {
          const photo: Photo = JSON.parse(response);
          this.member?.photos.push(photo);

          // set navbar profile photo when first photo is uploaded
          if (this.member?.photos.length === 1)
            this.setNavbarProfilePhoto(photo.url_165)
        }

        this.isUploading = false;
      }

      this.uploader.onErrorItem = (item, error) => {
        if (error)
          this.snackBar.open(error, 'Close', { horizontalPosition: 'center', verticalPosition: 'top', duration: 7000 });
      }
    }
  }
  //#endregion Photo Upload using `ng2-file-upload`

  /**
   * Set main photo for card and album
   * @param url_165In 
   */
  setMainPhoto(url_165In: string): void {
    this.userService.setMainPhoto(url_165In)
      .pipe(take(1))
      .subscribe({
        next: (response: ApiResponseMessage) => {
          if (response && this.member) {

            this.member.photos.forEach(photo => {
              // unset previous main
              if (photo.isMain === true)
                photo.isMain = false;

              // set new selected main
              if (photo.url_165 === url_165In && this.loggedInUser) {
                photo.isMain = true;

                // update navbar photo
                this.loggedInUser.profilePhotoUrl = url_165In;
                this.accountService.setCurrentUser(this.loggedInUser!);
              }
            })

            this.snackBar.open(response.message, 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000 });
          }
        }
      });
  }

  deletePhoto(url_165In: string, index: number): void {
    this.userService.deletePhoto(url_165In)
      .pipe(take(1))
      .subscribe({
        next: (pDR: PhotoDeleteResponse) => {
          if (this.member) {
            this.member.photos.splice(index, 1);

            // Update navbar if there's no photo left.
            if (this.member.photos.length === 0 && this.loggedInUser) {
              this.loggedInUser.profilePhotoUrl = undefined;
              this.accountService.setCurrentUser(this.loggedInUser);
            }

            // Update navbar if main photo was deleted
            if (pDR.newMainUrl)
              this.setNextMainWhenMainDeleted(pDR);

            this.snackBar.open(pDR.successMessage, 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000 });
          }
        }
      })
  }

  /**
   * Set navbar profile photo ONLY when FIRST photo is uploaded.
   * @param url_165 
   */
  private setNavbarProfilePhoto(url_165: string): void {
    if (this.loggedInUser) {

      this.loggedInUser.profilePhotoUrl = url_165;

      this.accountService.loggedInUserSig.set(this.loggedInUser)
    }
  }

  /**
   * If the main photo was deleted, set the next photo (if any) as the main photo.
   * @param pDR // has value when deleted photo was main. Update navbar photo
   */
  private setNextMainWhenMainDeleted(pDR: PhotoDeleteResponse): void {
    if (this.member) {
      this.setNavbarProfilePhoto(pDR.newMainUrl);

      // exclude the blob SasToken
      const imageUrlFirstPart = pDR.newMainUrl.split('.web');

      for (const photo of this.member.photos) {
        // If the deleted photo was main => Update the next photo as main.
        if (photo.url_165.includes(imageUrlFirstPart[0]))
          photo.isMain = true;
      }
    }
  }
}
