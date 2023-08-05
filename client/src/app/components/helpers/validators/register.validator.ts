import { AbstractControl, ValidationErrors } from "@angular/forms";

export class RegisterValidators {
    static confirmPassword(group: AbstractControl): ValidationErrors | null {
        const password = group.get('passwordCtrl')?.value as string;
        const confirmPassword = group.get('confirmPasswordCtrl')?.value as string;

        // console.log('password ' + password);
        // console.log('confirm ' + confirmPassword);

        return password !== confirmPassword ? { invalidPasswordsMatch: true } : null
    }
}