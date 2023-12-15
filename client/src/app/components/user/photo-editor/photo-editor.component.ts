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
import { User } from '../../../models/user.model';

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

  user: User | undefined;
  errorGlob: string | undefined;
  basePhotoUrl: string = environment.apiPhotoUrl;

  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;

  constructor() {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user)
          this.user = user
      }
    });
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(): void {
    this.uploader = new FileUploader({
      url: environment.apiUrl + 'user/add-photo',
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
    this.userService.setMainPhoto(url_128In).pipe(take(1)).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.modifiedCount === 1 && this.user && this.member) {
          this.member.photos.forEach(photo => {
            // unset previous main
            if (photo.isMain === true)
              photo.isMain = false;

            // set new selected main
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
    this.userService.deletePhoto(url_128In).pipe(take(1)).subscribe({
      next: (updateResult: UpdateResult) => {
        if (updateResult.modifiedCount === 1 && this.member) {
          this.member.photos.splice(index, 1);
        }
      }
    })
  }
}
