import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HttpClientModule } from '@angular/common/http';
import { ComponentModule } from './modules/component.module';
import { CountryListService } from './_services/country-list.service';
import { ErrorStateMatcher } from '@angular/material/core';
import { DefaultErrorStateMatcher } from './extensions/validators/default-error-state.matcher';

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
    { provide: ErrorStateMatcher, useClass: DefaultErrorStateMatcher }],
  bootstrap: [AppComponent]
})
export class AppModule { }
