import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from '../app-routing.module';
import { MaterialModule } from './material.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { NgOptimizedImage } from '@angular/common';
import { DirectiveModule } from './directive.module';

//pipes
import { ShortenStringPipe } from '../pipes/shorten-string.pipe';

// components
import { NoAccessComponent } from '../components/errors/no-access/no-access.component';
import { NotFoundComponent } from '../components/errors/not-found/not-found.component';
import { TestErrorComponent } from '../components/errors/test-error/test-error.component';
import { ServerErrorComponent } from '../components/errors/server-error/server-error.component';

import { HomeComponent } from '../components/home/home.component';
import { RegisterComponent } from '../components/account/register/register.component';
import { LoginComponent } from '../components/account/login/login.component';
import { NavbarComponent } from '../components/navbar/navbar.component';
import { ListsComponent } from '../components/lists/lists.component';
import { MessagesComponent } from '../components/messages/messages.component';
import { PhotoEditorComponent } from '../components/users/photo-editor/photo-editor.component';
import { UserListComponent } from '../components/users/user-list/user-list.component';
import { UserCardComponent } from '../components/users/user-card/user-card.component';
import { UserDetailComponent } from '../components/users/user-detail/user-detail.component';
import { UserEditComponent } from '../components/users/user-edit/user-edit.component';

// ControlValueAccessor helpers
import { InputCvaComponent } from '../components/helpers/input-cva/input-cva.component';
import { DatePickerCvaComponent } from '../components/helpers/date-picker-cva/date-picker-cva.component';


// 3rd party packages
import { NgxGalleryModule } from '@kolkov/ngx-gallery';
import { FileUploadModule } from 'ng2-file-upload';
import { HttpClientModule } from '@angular/common/http';

const components = [
  NoAccessComponent,
  NotFoundComponent,
  TestErrorComponent,
  ServerErrorComponent,

  HomeComponent,
  RegisterComponent,
  LoginComponent,
  NavbarComponent,
  UserListComponent,
  UserDetailComponent,
  ListsComponent,
  UserCardComponent,
  MessagesComponent,
  UserEditComponent,
  PhotoEditorComponent,

  ShortenStringPipe,

  InputCvaComponent,
  DatePickerCvaComponent
]

@NgModule({
  declarations: [components],
  imports: [
    CommonModule,
    AppRoutingModule, // also in app.module.ts
    HttpClientModule,
    BrowserAnimationsModule, // also in app.module.ts
    FormsModule,
    ReactiveFormsModule,
    MaterialModule, // do NOT import in app.module.ts
    DirectiveModule,
    NgOptimizedImage,
    NgxGalleryModule,
    FileUploadModule,
  ],
  exports: [components]
})
export class ComponentModule { }
