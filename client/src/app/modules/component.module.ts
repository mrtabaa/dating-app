import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from '../app-routing.module';
import { MaterialModule } from './material.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

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
import { DirectiveModule } from './directive.module';
import { ListsComponent } from '../components/lists/lists.component';
import { MemberDetailComponent } from '../components/members/member-detail/member-detail.component';
import { MemberListComponent } from '../components/members/member-list/member-list.component';
import { MessagesComponent } from '../components/messages/messages.component';

const components = [
  NoAccessComponent,
  NotFoundComponent,
  TestErrorComponent,
  ServerErrorComponent,
  
  HomeComponent,
  RegisterComponent,
  LoginComponent,
  NavbarComponent,
  MemberListComponent,
  MemberDetailComponent,
  ListsComponent,
  MessagesComponent,

  ShortenStringPipe
]

@NgModule({
  declarations: [components],
  imports: [
    CommonModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    DirectiveModule
  ],
  exports: [components]
})
export class ComponentModule { }
