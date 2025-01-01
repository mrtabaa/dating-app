import {AbstractControl, FormControl, ValidationErrors, ValidatorFn} from "@angular/forms";
import {inject} from "@angular/core";
import {GooglePlacesService} from "../services/google-places.service";

export function locationValidator(): ValidatorFn {
  const isCountrySelectedSig = inject(GooglePlacesService).isCountrySelectedSig;

  return (control: AbstractControl): ValidationErrors | null => {

    // Ensure the control is a FormControl
    if (!(control instanceof FormControl)) {
      return null;
    }

    return !isCountrySelectedSig() ? {invalidLocation: {value: true}} : null;
  };
}
