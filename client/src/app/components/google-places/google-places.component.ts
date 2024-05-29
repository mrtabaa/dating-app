import { NgOptimizedImage } from '@angular/common';
import { Component, Signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { GooglePlacesService } from '../../services/google-places.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-google-places',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    NgOptimizedImage, MatButtonModule, MatInputModule, MatCardModule, MatDividerModule,
    MatIconModule
  ],
  templateUrl: './google-places.component.html',
  styleUrl: './google-places.component.scss'
})
export class GooglePlacesComponent {
  private googlePlacesService = inject(GooglePlacesService);
  private fb = inject(FormBuilder);

  countrySig: Signal<string | undefined> = this.googlePlacesService.countrySig;
  countryAcrSig: Signal<string | undefined> = this.googlePlacesService.countryAcrSig;
  stateSig: Signal<string | undefined> = this.googlePlacesService.stateSig;
  citySig: Signal<string | undefined> = this.googlePlacesService.citySig;
  isCountrySelectedSig: Signal<boolean> = this.googlePlacesService.isCountrySelectedSig;

  isImageLoaded = false;

  searchedLocationCtrl = this.fb.control(null, Validators.required);

  searchLocation(location: HTMLInputElement): void {
    this.googlePlacesService.isCountrySelectedSig.set(false);
    this.googlePlacesService.countrySig.set(undefined);

    this.googlePlacesService.searchLocation(location);
  }

  confirmLocation(): void {
    this.googlePlacesService.isCountrySelectedSig.set(true);
    this.countrySig = this.googlePlacesService.countrySig;
    this.countryAcrSig = this.googlePlacesService.countryAcrSig;
    this.stateSig = this.googlePlacesService.stateSig;
    this.citySig = this.googlePlacesService.citySig;
  }

  resetCountry(): void {
    this.isImageLoaded = false;
    this.searchedLocationCtrl.reset();

    this.googlePlacesService.resetCountry();

    this.countrySig = this.googlePlacesService.countrySig;
    this.isCountrySelectedSig = this.googlePlacesService.isCountrySelectedSig;
  }
}
