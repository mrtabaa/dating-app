import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from '../app-routing.module';
import { MaterialModule } from './material.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

//pipes
import { ShortenStringPipe } from '../_pipes/shorten-string.pipe';

// components
import { HomeComponent } from '../components/home/home.component';
import { RegisterComponent } from '../components/account/register/register.component';
import { LoginComponent } from '../components/account/login/login.component';
import { NavbarComponent } from '../components/navbar/navbar.component';
import { NoAccessComponent } from '../components/no-access/no-access.component';
import { NotFoundComponent } from '../components/not-found/not-found.component';
import { DirectiveModule } from './directive.module';

const components = [
  HomeComponent,
  RegisterComponent,
  LoginComponent,
  NavbarComponent,
  NoAccessComponent,
  NotFoundComponent,

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
