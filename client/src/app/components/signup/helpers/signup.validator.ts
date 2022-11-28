import { AbstractControl, ValidationErrors } from "@angular/forms";

export class SignupValidators {

    // Validate form if all necessary fields are valid
    static validateForm(form: AbstractControl): ValidationErrors | null {
        // Lab Info
        const countryFilterCtrl = form.get('countryFilterCtrl');
        const selectedCountryCtrl = form.get('selectedCountryCtrl');
        const universityNameCtrl = form.get('universityNameCtrl');
        const governmentIdCtrl = form.get('governmentIdCtrl');
        const emailCtrl = form.get('emailCtrl');

        // Contact Info
        const phoneCountryCodeCtrl = form.get('phoneCountryCodeCtrl');
        const phoneNumberCtrl = form.get('phoneNumberCtrl');
        const combinedPhoneNumberCtrl = form.get('combinedPhoneNumberCtrl');
        const streetCtrl = form.get('streetCtrl');

        return !(countryFilterCtrl && countryFilterCtrl.valid
            && selectedCountryCtrl && selectedCountryCtrl.valid
            && universityNameCtrl && universityNameCtrl.valid
            && governmentIdCtrl && governmentIdCtrl.valid
            && emailCtrl && emailCtrl.valid
            && phoneCountryCodeCtrl && phoneCountryCodeCtrl.valid
            && phoneNumberCtrl && phoneNumberCtrl.valid
            && combinedPhoneNumberCtrl && combinedPhoneNumberCtrl.valid
            && streetCtrl && streetCtrl.valid)
            ? { invalidForm: true }
            : null;
    }

    static validateCountry(group: AbstractControl): ValidationErrors | null {
        const countryFilterCtrl = group.get('countryFilterCtrl');
        const selectedCountryCtrl = group.get('selectedCountryCtrl');

        if (group && selectedCountryCtrl?.invalid) {
            countryFilterCtrl?.setErrors({ invalidCountry: true })
        }
        return null;
    }
}