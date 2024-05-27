import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GooglePlaceService {
  countrySig = signal<string | undefined>(undefined);
  countryAcrSig = signal<string | undefined>(undefined);
  stateSig = signal<string | undefined>(undefined);
  citySig = signal<string | undefined>(undefined);

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

  /* called in searchUniversity() >> google.maps.event.addListenerOnce()
  ** sets: 
  **    country, state, city, zip
  */
  private setPlaceDetails(place: google.maps.places.PlaceResult): void {
    if (place) {
      const address_components = place.address_components as google.maps.GeocoderAddressComponent[];

      if (address_components) {
        this.countrySig.set(address_components[3].long_name);
        this.countryAcrSig.set(address_components[3].short_name);
        this.stateSig.set(address_components[2].long_name);
        this.citySig.set(address_components[0].long_name);
      }
    }
  }
}
