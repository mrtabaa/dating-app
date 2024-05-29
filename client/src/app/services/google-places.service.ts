import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GooglePlacesService {
  countrySig = signal<string | undefined>(undefined);
  countryAcrSig = signal<string | undefined>(undefined);
  stateSig = signal<string | undefined>(undefined);
  citySig = signal<string | undefined>(undefined);
  isCountrySelectedSig = signal<boolean>(false);

  /*
  ** called by CompleteProfile
  */
  searchLocation(searchedLocationInput: HTMLInputElement): void {
    // set search conditions
    const options = {
      // componentRestrictions: { country: selectedCountryAcr },
      fields: ["place_id", "name", "address_components"],
      strictBounds: false,
      // types: ["university", "school"],
      types: ['(cities)']
    };

    // invoke googleMapService
    const autocomplete = new google.maps.places.Autocomplete(searchedLocationInput, options);

    // use addListenerOnce handler so it removes itself after the first event
    google.maps.event.addListenerOnce(autocomplete, 'place_changed', () => {
      const place = autocomplete.getPlace(); // get the PlaceResult
      this.setPlaceDetails(place);

      // Invoke Angular Detection since the Google API is performed outside of the Angular Zone
      // if (place) {
      //   this.zone.run(() => { });
      // }
    });
  }

  /**
   * Resets signals which are used for conditions in the components. 
   */
  resetCountry(): void {
    this.countrySig.set(undefined);
    this.isCountrySelectedSig.set(false);
  }

  /* called in searchUniversity() >> google.maps.event.addListenerOnce()
  ** sets: 
  **    country, state, city, zip
  */
  private setPlaceDetails(place: google.maps.places.PlaceResult): void {
    if (place) {
      const address_components = place.address_components as google.maps.GeocoderAddressComponent[];

      if (address_components) {
        if (address_components[3]) {
          this.countrySig.set(address_components[3].long_name);
          this.countryAcrSig.set(address_components[3].short_name);
          this.stateSig.set(address_components[2].long_name);
          this.citySig.set(address_components[0].long_name);
        }
        else { // Ahvaz-khuzestan doesn't have [3]
          this.countrySig.set(address_components[2].long_name);
          this.countryAcrSig.set(address_components[2].short_name);
          this.stateSig.set(address_components[1].long_name);
          this.citySig.set(address_components[0].long_name);
        }
      }
    }
  }
}
