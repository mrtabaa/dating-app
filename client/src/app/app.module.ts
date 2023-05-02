import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ComponentModule } from './modules/component.module';
import { CountryListService } from './services/country-list.service';
import { ErrorStateMatcher } from '@angular/material/core';
import { DefaultErrorStateMatcher } from './extensions/validators/default-error-state.matcher';
import { ErrorInterceptor } from './interceptors/error.interceptor';
import { JwtInterceptor } from './interceptors/jwt.interceptor';

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,

    ComponentModule,
  ],
  providers: [
    CountryListService,
    { provide: ErrorStateMatcher, useClass: DefaultErrorStateMatcher },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
